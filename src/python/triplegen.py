from triple import Triple

class TripleGenerator:
    # Generates relationship-triples based on an input text.
    def generate(inputText: str) -> list[Triple]:

        # BIG TODO: Analyze text (with spaCy) and return the result

        exampleResult = [
            Triple('sun', 'be', 'star'),
            Triple('star', 'be', 'bright'),
        ]

        return exampleResult
    
