import spacy
from spacy import displacy
class Model:
    def runNLP(inputText):
        # Load the saved model
        output_dir="C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Adriana\\NER_NEU" #Pfad anpassen
        loaded_nlp = spacy.load(output_dir)

        # Test the loaded NER model
        doc = loaded_nlp(inputText)
        
        for token in doc:
                token.text
                token.lemma
                token.pos_
                token.tag_
                token.dep_,
                token.shape_
                token.is_alpha
                token.is_stop
        return doc.ents 