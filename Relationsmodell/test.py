import json

# Pfad zur Textdatei
input_file = "C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Relationsmodell\\data\\alicebob.txt"

# Pfad zur JSON-Ausgabedatei
output_file = "C:\\Users\\Adriana\\Desktop\\Uni\\Fachpraktikum\\Repos\\concept-maps\\Relationsmodell\\data\\alicebob.json"


# Liste zum Speichern der Sätze
data = []

# Textdatei lesen und Sätze extrahieren
with open(input_file, "r") as file:
    for i, line in enumerate(file):
        sentence = line.strip()
        tokens = sentence.split()
        token_objects = []
        sansa_token = None
        jon_token = None
        for j, token in enumerate(tokens):
            if token == "Sansa":
                entity_label = "PERSON"
                start = sentence.index(token)
                end = start + len(token) - 1
                sansa_token = j
                token_object = {
                    "text": token + " Stark",
                    "start": start,
                    "end": end+7,
                    "token_start": j,
                    "token_end": j+1,
                    "entityLabel": entity_label
                }
                token_objects.append(token_object)
            elif token == "Jon":
                entity_label = "PERSON"
                start = sentence.index(token)
                end = start + len(token) - 1
                jon_token = j
                token_object = {
                    "text": token + " Snow",
                    "start": start,
                    "end": end + 6,
                    "token_start": j,
                    "token_end": j+1,
                    "entityLabel": entity_label
                }
                token_objects.append(token_object)
        
        relation_objects = []
        if sansa_token is not None and jon_token is not None:
            relation_label = "SIBLINGS" if i < 20 else "undefined"
            relation_object = {
                "child": sansa_token,
                "head": jon_token,
                "relationLabel": relation_label
            }
            relation_objects.append(relation_object)
        
        sentence_object = {
            "sentence": sentence,
            "tokens": token_objects,
            "relations": relation_objects
        }
        
        data.append(sentence_object)

# JSON-Datei schreiben
with open(output_file, "w") as file:
    json.dump(data, file, indent=4)

print("Die JSON-Datei wurde erfolgreich erstellt: {}".format(output_file))