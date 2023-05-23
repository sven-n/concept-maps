﻿namespace ConceptMaps.Crawler;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Analyzes the text for possible sentences which include hints to the relationships
/// between persons.
/// It's using a naive approach, by simply checking for every possible relationship
/// in a sentence.
/// If both (fore) names of a relationship are included in the sentence, we assume the
/// sentence is about describing the relationship.
/// </summary>
internal class RelationshipAnalyzer
{
    /// <summary>
    /// Analyzes the text with the possible relationships and stores the results
    /// as a json file in a result file.
    /// </summary>
    /// <param name="textFilePath">The text file path.</param>
    /// <param name="relationshipFilePath">The relationship file path.</param>
    /// <param name="resultFilePath">The result file path.</param>
    public void AnalyzeAndStoreResults(string textFilePath, string relationshipFilePath, string resultFilePath)
    {
        var text = File.ReadAllText(textFilePath);
        var parsedRelationships = File.ReadAllLines(relationshipFilePath)
            .Select(line => line.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(tokens => tokens.Length == 3)
            .Select(tokens => new Relationship(tokens[0], tokens[1].ToLower(), tokens[2]))
            .Select(NormalizeRelationship)
            .Distinct()
            .ToList();

        var foundSentences = FindSentences(text, parsedRelationships);
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        };

        File.WriteAllText(resultFilePath, JsonSerializer.Serialize(foundSentences, serializerOptions), Encoding.UTF8);
    }

    /// <summary>
    /// Normalizes the relationship to be able to filter out duplicate information.
    /// E.g. A is father of B -> B is child of A.
    /// </summary>
    /// <param name="relationship">The relationship.</param>
    /// <returns>The normalized relationship.</returns>
    private static Relationship NormalizeRelationship(Relationship relationship)
    {
        switch (relationship.RelationshipType)
        {
            case "mother":
            case "father":
                return new Relationship(relationship.SecondEntity, "children", relationship.FirstEntity);
            default:
                return relationship;
        }
    }

    private static List<SentenceRelationships> FindSentences(string text, List<Relationship> possibleRelationships)
    {
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return sentences
            .Select(sentence => ProcessSentence(sentence, possibleRelationships))
            .Where(result => result.Relationships.Any())
            .ToList();
    }

    private static SentenceRelationships ProcessSentence(string sentence, List<Relationship> possibleRelationships)
    {
        return new SentenceRelationships(
            sentence,
            possibleRelationships.Where(relationship =>
                           (sentence.Contains(relationship.FirstEntity) || sentence.Contains(relationship.FirstEntityForeName)) 
                        && (sentence.Contains(relationship.SecondEntity) || sentence.Contains(relationship.SecondEntityForeName)))
            .ToList());
    }

    private record Relationship(string FirstEntity, string RelationshipType, string SecondEntity)
    {
        [JsonIgnore]
        public string FirstEntityForeName { get; } = FirstEntity.Split(' ').First();

        [JsonIgnore]
        public string SecondEntityForeName { get; } = SecondEntity.Split(' ').First();
    }

    private record SentenceRelationships(string Sentence, List<Relationship> Relationships);
}