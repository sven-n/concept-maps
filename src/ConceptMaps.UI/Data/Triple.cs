namespace ConceptMaps.UI.Data;

/// <summary>
/// Defines a triple which contains two words and their connection (<see cref="EdgeName"/>)
/// to each other as input for a directed graph.
/// </summary>
public record struct Triple(string FromWord, string? EdgeName, string ToWord, double Score = double.NaN);