namespace ConceptMaps.Tests;

using Blazor.Diagrams.Core.Models;
using ConceptMaps.DataModel;
using ConceptMaps.UI.Services;
using GraphShape;
using QuikGraph;

/// <summary>
/// Tests for the <see cref="FamilyTreeLayoutAlgorithm"/>.
/// </summary>
[TestClass]
public class FamilyTreeLayoutAlgorithmTest
{
    /// <summary>
    /// Tests determining the parents which are not children themselves.
    /// </summary>
    [TestMethod]
    public void GetParentsWithoutBeingChild()
    {
        var layoutAlgorithm = SetupAlgorithm();
        var parentLayers = layoutAlgorithm.GetParentsWithoutBeingChild();
        Assert.AreEqual(1, parentLayers.Count);
        Assert.AreEqual(3, parentLayers.First().Count);
    }

    /// <summary>
    /// Tests if adding the children of the parents to their corresponding layers work as expected.
    /// </summary>
    [TestMethod]
    public void AfterAddingChildren()
    {
        var layoutAlgorithm = SetupAlgorithm();
        var layers = layoutAlgorithm.GetParentsWithoutBeingChild();
        layoutAlgorithm.AddChildren(layers);
        Assert.AreEqual(2, layers.Count);
        Assert.AreEqual(3, layers[0].Count);
        Assert.AreEqual(3, layers[1].Count);
    }

    /// <summary>
    /// Tests if spouses of children are added to the same layer.
    /// </summary>
    [TestMethod]
    public void AfterAddingSpouses()
    {
        var layoutAlgorithm = SetupAlgorithm();
        var layers = layoutAlgorithm.GetParentsWithoutBeingChild();
        layoutAlgorithm.AddChildren(layers);
        layoutAlgorithm.AddSpouses(layers);
        Assert.AreEqual(2, layers.Count);
        Assert.AreEqual(4, layers[0].Count);
        Assert.AreEqual(3, layers[1].Count);
    }

    /// <summary>
    /// Tests if all nodes have distinct positions, so they don't overlap.
    /// </summary>
    [TestMethod]
    public void CalculatePositions_DistinctPositions()
    {
        var layoutAlgorithm = SetupAlgorithm();
        layoutAlgorithm.Compute();
        var positions = layoutAlgorithm.VerticesPositions;
            
        var distinctCount = positions.Values.Distinct().Count();

        Assert.AreEqual(positions.Count, distinctCount);
    }

    /// <summary>
    /// Tests if all nodes of the same layer are on the same row (Y coordinate).
    /// </summary>
    [TestMethod]
    public void CalculatePositions_SameLayerSameY()
    {
        var layoutAlgorithm = SetupAlgorithm();
        layoutAlgorithm.Compute();
        var graph = layoutAlgorithm.VisitedGraph;
        var positions = layoutAlgorithm.VerticesPositions;

        Point GetPosition(string name)
        {
            return positions[graph.Vertices.First(v => v.Id == name)];
        }

        var jeff = GetPosition("Jeff");
        var mary = GetPosition("Mary");
        var alex = GetPosition("Alex");

        var bob = GetPosition("Bob");
        var alice = GetPosition("Alice");
        var tom = GetPosition("Tom");

        var layer1Y = new[] { jeff.Y, mary.Y, alex.Y };
        var layer2Y = new[] { bob.Y, alice.Y, tom.Y };

        Assert.AreEqual(1, layer1Y.Distinct().Count());
        Assert.AreEqual(1, layer2Y.Distinct().Count());
    }

    /// <summary>
    /// Tests if spouses are directly added after each other.
    /// </summary>
    [TestMethod]
    public void CalculatePositions_JimAfterAlice()
    {
        var layoutAlgorithm = SetupAlgorithm();
        layoutAlgorithm.Compute();
        var graph = layoutAlgorithm.VisitedGraph;
        var positions = layoutAlgorithm.VerticesPositions;

        Point GetPosition(string name)
        {
            return positions[graph.Vertices.First(v => v.Id == name)];
        }

        var alice = GetPosition("Alice");
        var jim = GetPosition("Jim");
        
        Assert.AreEqual(alice.Y, jim.Y);
        Assert.IsTrue(alice.X < jim.X);
        Assert.IsFalse(positions.Values.Where(p => Math.Abs(p.Y - alice.Y) < 0.01).Any(p => p.X > alice.X && p.X < jim.X));
    }

    /// <summary>
    /// Tests if persons which are not inside another parent-child hierarchy are added as well.
    /// </summary>
    [TestMethod]
    public void CalculatePositions_Unrelated()
    {
        var layoutAlgorithm = SetupAlgorithm();
        layoutAlgorithm.Compute();
        var graph = layoutAlgorithm.VisitedGraph;
        var positions = layoutAlgorithm.VerticesPositions;

        Point GetPosition(string name)
        {
            return positions[graph.Vertices.First(v => v.Id == name)];
        }

        var kate = GetPosition("Kate");
        var bruce = GetPosition("Bruce");
        
        Assert.AreEqual(kate.Y, bruce.Y);
        Assert.AreNotEqual(kate.X, bruce.X);
    }

    private static FamilyTreeLayoutAlgorithm SetupAlgorithm()
    {
        var graph = MakeGraph(GetTestFamily());
        var sizes = graph.Vertices.ToDictionary(v => v, v => new Size(100, 80));
        return new FamilyTreeLayoutAlgorithm(graph, sizes);
    }

    private static BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>> MakeGraph(List<Triple> triples)
    {
        var graph = new BidirectionalGraph<NodeModel, TaggedEdge<NodeModel, string>>(false);
        var entities = triples.Select(t => t.FromWord).Concat(triples.Select(t => t.ToWord)).Distinct();
        var nodes = new Dictionary<string, NodeModel>();
        foreach (var entityName in entities)
        {
            var node = new NodeModel(entityName);
            node.Size = new Blazor.Diagrams.Core.Geometry.Size(100, 80);
            graph.AddVertex(node);
            nodes.Add(entityName, node);
        }

        foreach (var triple in triples)
        {
            graph.AddEdge(new TaggedEdge<NodeModel, string>(nodes[triple.FromWord], nodes[triple.ToWord],triple.EdgeName));
        }

        return graph;
    }

    private static List<Triple> GetTestFamily()
    {
        var result = new List<Triple>
        {
            new("Bob", "SIBLINGS", "Alice"),
            new("Bob", "SIBLINGS", "Tom"),
            new("Alice", "SIBLINGS", "Tom"),

            // Jeffs children:
            new("Bob", "CHILDREN", "Jeff"),
            new("Alice", "CHILDREN", "Jeff"),

            // Marys children:
            new("Bob", "CHILDREN", "Mary"),
            new("Alice", "CHILDREN", "Mary"),

            // Jeff and Mary are married
            new("Mary", "SPOUSE", "Jeff"),

            // Tom is child of Alex and Mary (they're not married):
            new("Tom", "CHILDREN", "Alex"),
            new("Tom", "CHILDREN", "Mary"),

            // Alice husband:
            new("Jim", "SPOUSE", "Alice"),

            // Unrelated:
            new ("Kate", "SIBLINGS", "Bruce"),
        };

        return result;
    }
}