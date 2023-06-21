import json
import random
import typer
from pathlib import Path

from spacy.tokens import Span, DocBin, Doc
from spacy.vocab import Vocab
from wasabi import Printer
from spacy.tokenizer import Tokenizer
from spacy.lang.en import English
from spacy.util import compile_infix_regex
import re
import spacy
import os

output_dir="C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Adriana\\NER_NEU"
nlp = spacy.load(output_dir)
#nlp = spacy.blank("en")
# Create a blank Tokenizer with just the English vocab
is_complete = True

msg = Printer()

SYMM_LABELS = ["Binds"]
DIRECTED_LABELS = {"CHILDREN","SPOUSE","SIBLINGS","undefined"}

pfad = os.getcwd()
ann = pfad + '\\alicebob.json'
train_file=pfad + '\\relations_training.spacy'
dev_file=pfad + '\\relations_dev.spacy'
test_file=pfad + '\\relations_test.spacy'

# TODO: define splits for train/dev/test. What is not in test or dev, will be used as train.
test_portion = 0.2
dev_portion = 0.3

def main(json_loc: Path, train_file: Path, dev_file: Path, test_file: Path):
    
    
    """Creating the corpus from the Prodigy annotations."""
    Doc.set_extension("rel", default={})
    failed_examples=[]
    
    i = 0
    j = 0
    vocab = Vocab()

    docs = {"train": [], "dev": [], "test": [], "total": []}
    ids = {"train": set(), "dev": set(), "test": set(), "total":set()}
    count_all = {"train": 0, "dev": 0, "test": 0,"total": 0}
    count_pos = {"train": 0, "dev": 0, "test": 0,"total": 0}

    with open(json_loc) as jsonfile:
        file = json.load(jsonfile)
        for example in file:
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
                entity = doc.char_span(
                        span['start'], span['end'], label=span['entityLabel']
                 )


                span_end_to_start[span["token_start"]] = span["token_start"]
                #print(span_end_to_start)
                entities.append(entity)
                span_starts.add(span["token_start"])
            
            try:
                doc.ents = entities
                i= i +1
            except:
                failed_examples.append(example)
                j = j +1
                
                #print('warning' + example['document'] + ' Teststatz. Will be ignored')

            # Parse the relations
            rels = {}
            for x1 in span_starts:
                for x2 in span_starts:
                    rels[(x1, x2)] = {}
                    #print(rels)
            relations = example["relations"]
            #print(len(relations))
            for relation in relations:
                # the 'head' and 'child' annotations refer to the end token in the span
                # but we want the first token
                start = span_end_to_start[relation["head"]]
                end = span_end_to_start[relation["child"]]
                label = relation["relationLabel"]
                #print(rels[(start, end)])
                #print(label)
                #label = MAP_LABELS[label]
                if label not in DIRECTED_LABELS:
                        msg.warn(f"Found label '{label}' not defined in SYMM_LABELS or DIRECTED_LABELS - skipping")
                        break
                if label not in rels[(start, end)]:
                    rels[(start, end)][label] = 1.0
                    pos += 1
                    #print(pos)
                    #print(rels[(start, end)])

            # The annotation is complete, so fill in zero's where the data is missing
            if is_complete:
                for x1 in span_starts:
                    for x2 in span_starts:
                        for label in DIRECTED_LABELS:
                            if label not in rels[(x1, x2)]:
                                neg += 1
                                rels[(x1, x2)][label] = 0.0

                                #print(rels[(x1, x2)])
                doc._.rel = rels
                #print(doc._.rel)

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

                    
                    
    #print(len(docs["total"]))
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
    

main(ann, train_file, dev_file, test_file)
