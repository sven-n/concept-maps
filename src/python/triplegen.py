"""
Generator which takes free text as input and returns a list of
triples of word relations.
"""

from triple import Triple
from model2 import Model2

model = Model2()

class TripleGenerator:
    """Generates relationship-triples based on an input text."""

    def generate(self, input_text: str) -> list[Triple]:
        """
        Generates relationship-triples based on an input text.
        :param input_text: The input text, given by the user.
        """

        result = model.get_triples(input_text)
        return result
