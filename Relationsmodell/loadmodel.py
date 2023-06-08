import spacy
# make the factory work
from rel_pipe import make_relation_extractor, score_relations

# make the config work
from rel_model import create_relation_model, create_classification_layer, create_instances

output_dir="C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\rel_component\\training\\model-best"
loaded_nlp = spacy.load(output_dir)

        # Test the loaded NER model
doc = loaded_nlp('Sansa Stark is the daughter of Eddard Stark.')        
for token in doc:
    token.text
    token.lemma
    token.pos_
    token.tag_
    token.dep_,
    token.shape_
    token.is_alpha
    token.is_stop
       