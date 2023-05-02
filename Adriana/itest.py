import networkx as nx
import matplotlib.pyplot as plt
from nltk.tokenize import word_tokenize
from nltk.stem import WordNetLemmatizer
from nltk.corpus import stopwords

# Lesen der Textdatei
with open("C:\\Users\\Adriana\\Desktop\\textdatei.txt", "r") as f:
    text = f.read()


# Tokenisierung des Texts
tokens = word_tokenize(text)

# Lemmatisierung der Tokens
lemmatizer = WordNetLemmatizer()
lemmatized_tokens = [lemmatizer.lemmatize(token.lower()) for token in tokens]

# Entfernen von Stoppwörtern und einzelnen Buchstaben
filtered_tokens = [token for token in lemmatized_tokens if token not in stopwords.words('english') and len(token) > 1]

# Extrahieren von Konzepten und Beziehungen aus den verbleibenden Tokens
concepts = set(filtered_tokens)
relationships = [(filtered_tokens[i], filtered_tokens[i+1]) for i in range(len(filtered_tokens)-1)]

# Erstellen der Concept Map als gerichteter Graph
G = nx.DiGraph()

# Hinzufügen von Knoten für jedes Konzept
for concept in concepts:
    G.add_node(concept)

# Hinzufügen von Kanten zwischen Konzepten, die im Text aufeinander folgen
for relationship in relationships:
    source, target = relationship
    if G.has_edge(source, target):
        G[source][target]['weight'] += 1
    else:
        G.add_edge(source, target, weight=1)

# TA-ARM-Algorithmus zur Verbesserung der Concept Map
for node1 in G.nodes():
    for node2 in G.nodes():
        if node1 != node2 and not G.has_edge(node2, node1):
            common_successors = set(G.successors(node1)).intersection(set(G.successors(node2)))
            if len(common_successors) > 0:
                for common_successor in common_successors:
                    G.add_edge(node1, node2, weight=G.out_degree(common_successor))

# Normalisierung der Knotengröße basierend auf der Anzahl der Knoten
node_size = 1000/len(G.nodes())

# Normalisierung der Kantengröße basierend auf der Anzahl der Kanten
edge_width = [0.5 * G[u][v]['weight']/max([G[i][j]['weight'] for i,j in G.edges]) for u,v in G.edges()]

# Zeichnen der Concept Map
pos = nx.spring_layout(G)
nx.draw_networkx(G, pos, with_labels=True, node_size=node_size, width=edge_width)
plt.axis('off')
plt.show()