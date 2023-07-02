from triple import Triple
import spacy
import random
import typer
from pathlib import Path
import spacy
from spacy.tokens import DocBin, Doc
from spacy.training.example import Example
from rel_pipe import make_relation_extractor, score_relations
from rel_model import create_relation_model, create_classification_layer, create_instances, create_tensors

class Model2:
    @staticmethod
    def runNLP(inputText):
        
        liste_triple=[]
        text = [inputText[1:-1]] # muss im Input angepasst werden
        ner = spacy.load("en_core_web_sm")
        
        # We load the relation extraction (REL) model
        nlp2 = spacy.load("training/relations/training/model-best")
            # We take the entities generated from the NER pipeline and input them to the REL pipeline
        for doc in ner.pipe(text):
            for name, proc in nlp2.pipeline:
                doc = proc(doc)
                # Here, we split the paragraph into sentences and apply the relation extraction for each pair of entities found in each sentence.
                for value, rel_dict in doc._.rel.items():
                    for sent in doc.sents:
                        for e in sent.ents:
                            for b in sent.ents:
                                if e.start == value[0] and b.start == value[1]:
                                    max_label = max(rel_dict, key=rel_dict.get)
                                    token_head = e.text
                                    token_child = b.text
                                    relation = max_label
                                    print([token_head, relation, token_child ])
                                    liste_triple.append(Triple(token_head, relation, token_child))
                           

        return liste_triple
                        