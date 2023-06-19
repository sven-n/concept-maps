from triple import Triple
from model2 import Model2

class TripleGenerator:
    # Generates relationship-triples based on an input text.
    def generate(self, inputText: str) -> list[Triple]:
        # BIG TODO: Analyze text (with spaCy) and return the result
        exampleResult = Model2.runNLP(inputText[0])

        return exampleResult
    
