import spacy
spans =[]
nlp = spacy.load("C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Adriana\\NER_NEU")
text=['Jamie Lannister Cersei Lannister']
for doc in nlp.pipe(text):
   #print(f"spans: {[(e.start, e.text, e.label_) for e in doc.ents]}")
   spans = doc.ents

import random
import typer
from pathlib import Path
import spacy
from spacy.tokens import DocBin, Doc
from spacy.training.example import Example
from rel_pipe import make_relation_extractor, score_relations
from rel_model import create_relation_model, create_classification_layer, create_instances, create_tensors
# We load the relation extraction (REL) model
nlp2 = spacy.load("C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Relationsmodell\\training\\model-best")
# We take the entities generated from the NER pipeline and input them to the REL pipeline
for name, proc in nlp2.pipeline:
          doc = proc(doc)
# Here, we split the paragraph into sentences and apply the relation extraction for each pair of entities found in each sentence.
for value, rel_dict in doc._.rel.items():
        for sent in doc.sents:
          for e in sent.ents:
            for b in sent.ents:
              if e.start == value[0] and b.start == value[1]:
                if rel_dict['CHILDREN'] >=0.9 :
                  print(f" entities: {e.text, b.text} --> predicted relation: {rel_dict}")
                  token_head = e.text
                  token_child = b.text
                  relation = 'child'
                if rel_dict['SPOUSE'] >=0.9 :
                  print(f" entities: {e.text, b.text} --> predicted relation: {rel_dict}")
                  token_head = e.text
                  token_child = b.text
                  relation = 'spouse'
                if rel_dict['SIBLINGS'] >=0.9 :
                  print(f" entities: {e.text, b.text} --> predicted relation: {rel_dict}")
                  token_head = e.text
                  token_child = b.text
                  relation = 'siblings'

