import spacy
import json
from spacy.tokens import Token
from spacy.language import Language


# Laden des Basismodells
nlp = spacy.load("en_core_web_sm")

# Einstiegsmethode:
def convertJsonFile(inputJsonPath: str, outputTraininigDataJsonPath: str):
    with open(inputJsonPath, "r", encoding="utf-8-sig") as inputFile:
        input = json.load(inputFile)
    result = convert(input)
    with open(outputTraininigDataJsonPath, "w") as outputFile:
        json.dump(result, outputFile, indent=True)

def convert(jsonObj : str):
    results = []
    for jsonSentence in jsonObj:
        sentence = jsonSentence['sentence']
        relationships = []
        for jsonRelationship in jsonSentence['relationships']:
            relationships.append((
                jsonRelationship['firstEntity'],
                jsonRelationship['relationshipType'],
                jsonRelationship['secondEntity']))
        processedSentence = process_sentence(sentence, relationships)
        results.append(processedSentence)
    return results

def process_sentence(sentence: str, relationships : list[(str, str, str)]):
    doc = nlp(sentence)
    
    tokens = []
    names1 = set([r[0] for r in relationships])
    names2 = set([r[2] for r in relationships])
    first_names1 = set([n.split()[0] for n in names1])
    first_names2 = set([n.split()[0] for n in names2])
    entity_names = names1.union(names2).union(first_names1).union(first_names2)
    entity_names = list(entity_names)
    entity_names.sort(reverse=True)
    last_entity_name = ''
    for entity_name in entity_names:
        if last_entity_name.find(entity_name) == -1:
            if add_entity_tokens(doc, sentence, entity_name, tokens):
                last_entity_name = entity_name

    relations = []
    
    for relationship in relationships:
        first_entity = relationship[0]
        relationship_type = relationship[1]
        second_entity = relationship[2]

        first_entity_token = find_token_by_name(tokens, first_entity)
        second_entity_token = find_token_by_name(tokens, second_entity)
        if first_entity_token != None and second_entity_token != None:
            relation = {
                "child": first_entity_token["start_token"],
                "head": second_entity_token["start_token"],
                "relationLabel": relationship_type.upper()
            }
            relations.append(relation)

    return {
        "document": sentence,
        "tokens": tokens,
        "relations": relations
    }

def find_token_by_name(tokens: list, name: str):
    for token in tokens:
        if (token["text"] == name):
            return token
    firstName = name.split()[0] # Nochmal nur nach Vornamen suchen
    for token in tokens:
        if (token["text"] == firstName):
            return token

def add_entity_tokens(doc: Language, sentence: str, entity_name: str, tokens: list):
    foundAny = False
    lastFoundIndex = sentence.find(entity_name)
    while lastFoundIndex >= 0:
        endIndex = lastFoundIndex + len(entity_name)
        start_token = find_token_by_start(doc, lastFoundIndex)
        end_token = find_token_by_end(doc, endIndex)
        if start_token != None and end_token != None:
            token = {
                "text" : entity_name,
                "start": lastFoundIndex,
                "end": endIndex,
                "start_token":  start_token.i,
                "end_token": end_token.i,
                "entityLabel": "PERSON"
                }
            
            tokens.append(token)
            foundAny = True
        lastFoundIndex = sentence.find(entity_name, endIndex)
    return foundAny

def find_token_by_start(doc: Language, startIndex: int) -> Token:
    for token in doc:
        if token.idx == startIndex:
            return token
        
def find_token_by_end(doc: Language, endIndex: int) -> Token:
    for token in doc:
        if token.idx + len(token.text) == endIndex:
            return token
