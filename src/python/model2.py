from triple import Triple
import spacy
import random
import typer
from pathlib import Path
import spacy
from spacy.tokens import DocBin, Doc, Span
from spacy.language import Language
from rel_pipe import make_relation_extractor, score_relations
from rel_model import create_relation_model, create_classification_layer, create_instances, create_tensors

class Model2:

    def __init__(self):
        self.ner_model : Language = spacy.load("en_core_web_sm")
        self.rel_model : Language = spacy.load("training/relations/training/model-best")

    def set_rel_model(self, model_path: (str|Path)):
        self.rel_model = spacy.load(model_path)
    
    def get_triples(self, inputText) -> list:
        result = []
        text = [inputText[1:-1]] # muss im Input angepasst werden
        
        for doc in self.ner_model.pipe(text):
            filtered_ents = [e for e in doc.ents if e.label_ == "PERSON"]
            doc.ents = filtered_ents
            for name, proc in self.rel_model.pipeline:
                doc = proc(doc)
            result.extend(self.process_document(doc))
        return result
    
    
    def process_document(self, doc: Doc) -> list:
        result : list = []
        pairs : list = []

        for sent in doc.sents:
            for a in sent.ents:
                for b in sent.ents:
                    if a.text > b.text:
                        pairs.append((a, b))

        for pair in pairs:
            a_to_b_dict = doc._.rel.get((pair[0].start, pair[1].start))
            b_to_a_dict = doc._.rel.get((pair[1].start, pair[0].start))

            if (a_to_b_dict is None and b_to_a_dict is None):
                continue

            if (b_to_a_dict is None):
                label = max(a_to_b_dict, key=a_to_b_dict.get)
                result.append(Triple(pair[0].text, label, pair[1].text))
                continue

            if (a_to_b_dict is None):
                label = max(b_to_a_dict, key=b_to_a_dict.get)
                result.append(Triple(pair[1].text, max_label_b, pair[0].text))
                continue

            # If we get results for both directions, we take the one with the highest score
            max_label_a = max(a_to_b_dict, key=a_to_b_dict.get)
            max_label_b = max(b_to_a_dict, key=b_to_a_dict.get)
            label_a_score = a_to_b_dict[max_label_a]
            label_b_score = b_to_a_dict[max_label_b]

            if (label_a_score > label_b_score):
                result.append(Triple(pair[0].text, max_label_a, pair[1].text))
            else:
                result.append(Triple(pair[1].text, max_label_b, pair[0].text))

        return result