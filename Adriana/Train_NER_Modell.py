import spacy
from spacy.training.example import Example 
import random
import os
from NerTrainingData import training_data_got # Wie genau kommen die Daten aus dem Crawler raus?
from spacy.tokens import Doc
training_data_got = training_data_got[0:250]  # damit das Modell schneller trainiert


nlp = spacy.load("en_core_web_lg")

# Create and add the NER pipeline component
ner = nlp.get_pipe("ner")

for text, annotations in training_data_got:
    doc =  Doc(vocab=nlp.vocab, words=[str(text)])
    # Initialize the optimizer
    optimizer = nlp.create_optimizer()

# Perform the training
for iteration in range(30):  #10 für die kleine Anzahl. Ansonsten könnte man ja so lange iterieren, bis die Fehler eine gewisse Untergrenze erreicht haben
    random.shuffle(training_data_got)
    losses = {}
    for text, annotations in training_data_got:
        doc =  Doc(vocab=nlp.vocab, words=[str(text)])
        try:
            example = Example.from_dict(doc, annotations)
            nlp.update([example], losses=losses, sgd=optimizer)
        except:
            print("Fehler, Sätz hat kein richtigen NER Entitäten definiert.")
            
            
# Save the trained NER model
output_dir = os.getcwd()
output_dir = output_dir + "\\NER_NEU"
nlp.to_disk(output_dir)

# Load the saved model
loaded_nlp = spacy.load(output_dir)
