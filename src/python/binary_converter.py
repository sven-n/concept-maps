"""Converter which takes the json file and creates the binary training data file."""

import json
import random
from pathlib import Path
from typing import Tuple
import spacy
from spacy.language import Language
from spacy.tokens import DocBin, Doc, Token
from spacy.vocab import Vocab

from wasabi import Printer

msg = Printer()

SYMM_LABELS = ["Binds"]
DIRECTED_LABELS = {"CHILDREN", "SPOUSE", "SIBLINGS", "undefined"}

nlp = spacy.load("en_core_web_sm") # we need the NLP parse sentence strings to tokens
Doc.set_extension("rel", default={}, force=True)
vocab = Vocab()

def create_relation_training_files(training_data: list[dict], output_dir: Path,
                                   test_portion: float = 0.2, dev_portion : float = 0.3):
    """Creating the corpus from the annotations."""

    failed_examples = []
    docs = {"train": [], "dev": [], "test": [], "total": []}

    random.shuffle(training_data)
    for sentence in training_data:
        try:
            is_simple = "relationships" in sentence
            if is_simple:
                (doc, contains_relation) = parse_simple_sentence(sentence)
            else:
                (doc, contains_relation) = parse_sentence(sentence)

            # only keeping documents with at least 1 positive case
            if contains_relation:
                randomized_append(docs, doc, test_portion, dev_portion)
        except Exception as ex:
            failed_examples.append(sentence)
            msg.fail(f"failed to include '{sentence['sentence']}' in the training data set. Exception: {ex}")

    save_spacy_docs(docs, output_dir)

def randomized_append(docs: dict[str, list], doc: Doc, test_portion: float, dev_portion : float):
    """Randomly adds the document to the docs dictionary, to test, dev or train."""
    docs["total"].append(doc)
    if random.random() < test_portion:
        docs["test"].append(doc)
    elif random.random() < (test_portion + dev_portion):
        docs["dev"].append(doc)
    else:
        docs["train"].append(doc)

def parse_sentence(example: dict) -> Tuple[Doc, bool]:
    """
    Parses the sentence data into a spaCy Doc.
    
    :return: Tuple of the parsed document, and a value if the document contains a relation.
    """

    tokens=nlp(example["sentence"])
    spaces=[]
    spaces = [True if tok.whitespace_ else False for tok in tokens]
    words = [t.text for t in tokens]
    doc = Doc(nlp.vocab, words=words, spaces=spaces)

    span_starts = set()
    parse_entities(doc, example["tokens"], span_starts)
    rels = prepare_rels(doc, span_starts)
    contains_relations = parse_relations(rels, example)
    return doc, contains_relations

def parse_simple_sentence(example: dict) -> Tuple[Doc, bool]:
    """
    Parses the "simple" sentence data into a spaCy Doc.
    The "simple" json format just contain the sentence and each relation, but without
    any start/end indexes.
    
    :return: Tuple of the parsed document, and a value if the document contains a relation.
    """

    tokens = nlp(example["sentence"])
    spaces=[]
    spaces = [True if tok.whitespace_ else False for tok in tokens]
    words = [t.text for t in tokens]
    doc = Doc(nlp.vocab, words=words, spaces=spaces)
    
    span_starts = set()
    entity_tokens = extract_entity_tokens(doc, example["relationships"])
    parse_entities(doc, entity_tokens, span_starts)
    rels = prepare_rels(doc, span_starts)
    contains_relations = parse_relations_simple(rels, example, entity_tokens)

    return doc, contains_relations

def prepare_rels(doc: Doc, span_starts: list[int]) -> dict:
    "Prepares the dictionary for the relations."
    rels = {}
    for i in span_starts:
        for j in span_starts:
            rels[(i, j)] = {}
            for label in DIRECTED_LABELS:
                rels[(i, j)][label] = 0.0
    doc._.rel = rels
    return rels

def parse_relations_simple(rels: dict, example, entity_tokens: list[str]) -> bool:
    """Parses the relations for the "simple" json format."""
    positives = 0
    for relationship in example["relationships"]:
        label = str.upper(relationship['relationshipType'])

        first_entity_token = find_token_by_name(entity_tokens, relationship['firstEntity'])
        second_entity_token = find_token_by_name(entity_tokens, relationship['secondEntity'])
        if first_entity_token is not None and second_entity_token is not None:
            child_start = first_entity_token["token_start"]
            head_start = second_entity_token["token_start"]
            start = head_start
            end = child_start
            if label not in DIRECTED_LABELS:
                msg.warn(f"Found label '{label}' not defined in DIRECTED_LABELS - skipping")
                break
            rels[(start, end)][label] = 1.0
            positives += 1
    return positives > 0

def parse_relations(rels: dict, example) -> bool:
    """Parses the relations for the "normal" json format."""
    positives = 0
    relations = example["relations"]
    for relation in relations:
        # the 'head' and 'child' annotations refer to the end token in the span
        # but we want the first token
        start = relation["head"]
        end =  relation["child"]
        label = relation["relationLabel"]
        if label not in DIRECTED_LABELS:
            msg.warn(f"Found label '{label}' not defined in DIRECTED_LABELS - skipping")
            break
        rels[(start, end)][label] = 1.0
        positives += 1
    return positives > 0

def extract_entity_names(relationships: list[dict[str, str]]) -> list:
    """Extracts all possible entity names from the given relationships."""
    names1 = set([r['firstEntity'] for r in relationships])
    names2 = set([r['secondEntity'] for r in relationships])
    first_names1 = set([n.split()[0] for n in names1])
    first_names2 = set([n.split()[0] for n in names2])
    entity_names = names1.union(names2).union(first_names1).union(first_names2)
    entity_names = list(entity_names)
    entity_names.sort(reverse=True)
    return entity_names

def extract_entity_tokens(doc: Doc, relationships: list[dict[str, str]]) -> list[dict]:
    """Extracts all entity tokens of the given relationships."""
    tokens = []
    entity_names = extract_entity_names(relationships)
    last_entity_name = ''
    for entity_name in entity_names:
        if last_entity_name.find(entity_name) == -1:
            if add_entity_tokens(doc, entity_name, tokens):
                last_entity_name = entity_name
    return tokens

def add_entity_tokens(doc: Doc, entity_name: str, tokens: list[dict]) -> bool:
    """Adds the tokens of the given entity name to the given tokens list."""
    found_any = False

    last_found_index = doc.text.find(entity_name)
    while last_found_index >= 0:
        end_index = last_found_index + len(entity_name)
        start_token = find_token_by_start(doc, last_found_index)
        end_token = find_token_by_end(doc, end_index)
        if start_token is not None and end_token is not None:
            token = {
                "text" : entity_name,
                "start": last_found_index,
                "end": end_index,
                "token_start":  start_token.i,
                "token_end": end_token.i,
                }
            
            tokens.append(token)
            found_any = True
        last_found_index = doc.text.find(entity_name, end_index)
    return found_any

def find_token_by_name(tokens: list, name: str):
    """Finds a token by name of the entity."""
    for token in tokens:
        if token["text"] == name:
            return token

    # If not found, search just for first name again
    first_name = name.split()[0]
    for token in tokens:
        if token["text"] == first_name:
            return token

def find_token_by_start(doc: Doc, start_index: int) -> (Token|None):
    """Finds the document token by start index."""
    for token in doc:
        if token.idx == start_index:
            return token

def find_token_by_end(doc: Doc, end_index: int) -> (Token|None):
    """Finds the document token by end index"""
    for token in doc:
        if token.idx + len(token.text) == end_index:
            return token

def parse_entities(doc: Doc, entity_tokens: list[dict], span_starts: set):
    """Parses the entities and sets it in the given document."""
    entities = []
    for entity_token in entity_tokens:
        start = entity_token['start']
        end = entity_token['end']
        entity = doc.char_span(start, end, label = "PERSON")
        entities.append(entity)
        span_starts.add(entity_token["token_start"])
    doc.ents = entities

def save_spacy_docs(docs: dict[str, list], output_dir: Path):
    """Saves the sentences in the spacy format."""
    train_file = output_dir.joinpath('relations_training.spacy')
    dev_file = output_dir.joinpath('relations_dev.spacy')
    test_file = output_dir.joinpath('relations_test.spacy')

    save_spacy_doc(docs, "train", train_file)
    save_spacy_doc(docs, "dev", dev_file)
    save_spacy_doc(docs, "test", test_file)

def save_spacy_doc(docs: dict[str, list], name: str, path: Path):
    """Saves the sentences to the spacy file."""
    docbin = DocBin(docs=docs[name], store_user_data=True)
    docbin.to_disk(path)

    if len(docs['total']) == 0:
        msg.warn("no sentences saved!")
    else:
        msg.info(f"{len(docs[name])} {name} sentences, {len(docs[name]) / len(docs['total'])} pos instances.")
