"""Contains the triple class which describes a relation between two words."""

class Triple:
    """The triple which describes a relation between two words."""

    def __init__(self, from_word : str, edge_name : str, to_word : str):
        self.fromWord = from_word
        self.edgeName = edge_name
        self.toWord = to_word
    