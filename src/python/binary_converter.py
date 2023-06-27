"""Converter which takes the json file and creates the binary training data file."""

import json
import random
from pathlib import Path
import spacy
from spacy.tokens import DocBin, Doc

from wasabi import Printer

msg = Printer()

SYMM_LABELS = ["Binds"]
DIRECTED_LABELS = {"CHILDREN","SPOUSE","SIBLINGS","undefined"}

def create_relation_training_files(json_str: str, output_dir: Path, test_portion: float = 0.2, dev_portion : float = 0.3):
    """Creating the corpus from the Prodigy annotations."""

    # json_annotation_file = output_dir.joinpath(json_loc)
    train_file = output_dir.joinpath('relations_training.spacy')
    dev_file = output_dir.joinpath('relations_dev.spacy')
    test_file = output_dir.joinpath('relations_test.spacy')

    # we need the NLP for tokenization
    nlp = spacy.load("en_core_web_sm")

    Doc.set_extension("rel", default={})
    failed_examples=[]
    docs = {"train": [], "dev": [], "test": [], "total": []}
    ids = {"train": set(), "dev": set(), "test": set(), "total":set()}
    count_all = {"train": 0, "dev": 0, "test": 0,"total": 0}
    count_pos = {"train": 0, "dev": 0, "test": 0,"total": 0}

    json_sentences_array = json.loads(json_str)
    for example in json_sentences_array:
        span_starts = set()
        neg = 0
        pos = 0

        # Parse the tokens
        tokens=nlp(example["sentence"])

        spaces=[]
        spaces = [True if tok.whitespace_ else False for tok in tokens]
        words = [t.text for t in tokens]
        doc = Doc(nlp.vocab, words=words, spaces=spaces)

        # Parse the GGP entities
        spans = example["tokens"]
        entities = []
        span_end_to_start = {}
        for span in spans:
            t1 = span['start']
            t2 = span['end']
            t3 = label=span['entityLabel']
            entity = doc.char_span(span['start'], span['end'], label=span['entityLabel'])
            span_end_to_start[span["token_start"]] = span["token_start"]
            entities.append(entity)
            span_starts.add(span["token_start"])
        try:
            doc.ents = entities
        except:
            failed_examples.append(example)

        # Parse the relations
        rels = {}
        for x1 in span_starts:
            for x2 in span_starts:
                rels[(x1, x2)] = {}
                
        relations = example["relations"]
        
        for relation in relations:
            # the 'head' and 'child' annotations refer to the end token in the span
            # but we want the first token
            start = span_end_to_start[relation["head"]]
            end = span_end_to_start[relation["child"]]
            label = relation["relationLabel"]
            if label not in DIRECTED_LABELS:
                msg.warn(f"Found label '{label}' not defined in SYMM_LABELS or DIRECTED_LABELS - skipping")
                break
            if label not in rels[(start, end)]:
                rels[(start, end)][label] = 1.0
                pos += 1

        # The annotation is complete, so fill in zero's where the data is missing
        for x1 in span_starts:
            for x2 in span_starts:
                for label in DIRECTED_LABELS:
                    if label not in rels[(x1, x2)]:
                        neg += 1
                        rels[(x1, x2)][label] = 0.0

                        #print(rels[(x1, x2)])
        doc._.rel = rels

        # only keeping documents with at least 1 positive case
        if pos > 0:
            if random.random() < test_portion:
                docs["test"].append(doc)
                count_pos["test"] += pos
                count_all["test"] += pos + neg
            elif random.random() < (test_portion + dev_portion):
                docs["dev"].append(doc)
                count_pos["dev"] += pos
                count_all["dev"] += pos + neg
            else:
                docs["train"].append(doc)
                count_pos["train"] += pos
                count_all["train"] += pos + neg

    docbin = DocBin(docs=docs["train"], store_user_data=True)
    docbin.to_disk(train_file)
    msg.info(
        f"{len(docs['train'])} training sentences, "
        f"{count_pos['train']}/{count_all['train']} pos instances."
    )

    docbin = DocBin(docs=docs["dev"], store_user_data=True)
    docbin.to_disk(dev_file)
    msg.info(
        f"{len(docs['dev'])} dev sentences, "
        f"{count_pos['dev']}/{count_all['dev']} pos instances."
    )

    docbin = DocBin(docs=docs["test"], store_user_data=True)
    docbin.to_disk(test_file)
    msg.info(
        f"{len(docs['test'])} test sentences, "
        f"{count_pos['test']}/{count_all['test']} pos instances."
    )
