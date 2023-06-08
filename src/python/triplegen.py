from triple import Triple
from model import Model

class TripleGenerator:
    # Generates relationship-triples based on an input text.
    def generate(self, inputText: str) -> list[Triple]:
        # BIG TODO: Analyze text (with spaCy) and return the result
        token = Model.runNLP(inputText[0])

        exampleResult = [
            Triple(token[0].text, 'child', token[1].text) # Relation muss angepasst werden. In der Relation werden auch die Entitäten gespeichert
        ]

        return exampleResult
    
