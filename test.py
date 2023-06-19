import spacy

# Laden des Spacy-Modells
nlp = spacy.load("en_core_web_md")

satz1 = "Sansa Stark is the sister of Jon Snow."
satz2 = "Arya Stark is the child of Eddard Stark."
satz3 = "Eddard Stark and Cately Stark are married."
sätze = []
sätze = [satz1,satz2,satz3]

eingabe = "Adriana is going on a walk with Elisa."

# Berechnung der Ähnlichkeit mit jedem Satz
for satz in sätze:
    sim= nlp(eingabe).similarity(nlp(satz))
    if sim > 0.75:
        print("Der Satz ist ähnlich.", sim)
        break
    else:
        print("Der Satz ist nicht ähnlich.", sim)
