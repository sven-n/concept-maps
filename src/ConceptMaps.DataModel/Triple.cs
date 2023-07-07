namespace ConceptMaps.DataModel;

/// <summary>
/// Defines a triple which contains two words and their connection (<see cref="EdgeName"/>)
/// to each other as input for a directed graph.
/// </summary>
/// <remarks>
/// Data model between Python service and .NET application.
/// </remarks>
public record struct Triple(string FromWord, string? EdgeName, string ToWord, double Score = double.NaN);