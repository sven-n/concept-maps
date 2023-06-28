namespace ConceptMaps.Crawler;

using System.Collections.Immutable;
using System.Text;

/// <summary>
/// Generator for training data of the Named Entity Recognition.
/// It takes the names (full and first) based on the known relationships
/// and searches for them in the sentences of the full text.
/// </summary>
public class NerTrainingDataGenerator
{
    private readonly ImmutableList<string> _entityNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="NerTrainingDataGenerator"/> class.
    /// </summary>
    /// <param name="relationshipFilePath">The relationship file path.</param>
    public NerTrainingDataGenerator(string relationshipFilePath)
    {
        this._entityNames = File.ReadAllLines(relationshipFilePath)
            .Select(line => line.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(tokens => tokens.Length == 3)
            .Select(tokens =>
            {
                tokens[0] = tokens[0].GetCleanEntityName();
                tokens[2] = tokens[2].GetCleanEntityName();
                return tokens;
            })
            .SelectMany(tokens => new[] { tokens[0], tokens[2] }
                .Append(tokens[0].Split(' ').First()) // Die Vornamen auch einzeln
                .Append(tokens[2].Split(' ').First()))
            .Distinct()
            .OrderDescending() // Absteigend sortieren, damit die vollen Namen zuerst kommen
            .ToImmutableList();
    }

    /// <summary>
    /// Generates the training data file based on the text and the known entity names.
    /// </summary>
    /// <param name="textFilePath">The text file path.</param>
    /// <returns>The path to the created file.</returns>
    public string GenerateTrainingDataFile(string textFilePath)
    {
        var sourceName = textFilePath.Split('_').FirstOrDefault() ?? string.Empty;
        var text = File.ReadAllText(textFilePath);
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(FindSentenceEntities)
            .Where(s => s.Entities.Any())
            .ToList();
        var trainingDataPythonCode = this.CreateTrainingCode(sentences, sourceName);

        var sourceFileName = textFilePath.Replace("_Text.txt", "_NerTrainingData.py");
        File.WriteAllText(sourceFileName, trainingDataPythonCode, Encoding.UTF8);
        return sourceFileName;
    }

    /// <remarks>
    /// Example:
    /// train_data_... = [
    /// ("""Tom plays tennis.""", {
    ///    "entities": [(0, 3, "PERSON")]
    /// }),...
    /// ]
    /// </remarks>
    private string CreateTrainingCode(List<SentenceEntities> sentences, string sourceName)
    {
        const string entityType = "\"PERSON\"";
        const string quotedQuote = "\\\"";

        var result = $"training_data_{sourceName} = [";

        var sentenceElements = sentences.Select(sentence =>
        {
            var entities = string.Join(", ", sentence.Entities.Select(e => $"({e.StartIndex}, {e.EndIndex}, {entityType})"));
            var sentenceElement = $"(\"\"\"{sentence.Sentence.Replace("\"", quotedQuote)}.\"\"\", {{\r\n";
            sentenceElement += "    \"entities\": [";
            sentenceElement += entities;
            sentenceElement += "]\r\n})";
            return sentenceElement;
        });

        result += string.Join(",\r\n", sentenceElements);
        result += "\r\n]";
        return result;
    }

    private SentenceEntities FindSentenceEntities(string sentence)
    {
        const StringComparison comparison = StringComparison.InvariantCulture;

        IList<NamedEntity>? entityNames = null;
        string lastEntityName = string.Empty;
        foreach (var entityName in this._entityNames)
        {
            if (lastEntityName.Contains(entityName))
            {
                continue;
            }

            var lastFoundIndex = sentence.IndexOf(entityName, comparison);
            while (lastFoundIndex >= 0)
            {
                var endIndex = lastFoundIndex + entityName.Length;
                var namedEntity = new NamedEntity(entityName, lastFoundIndex, endIndex);
                entityNames ??= new List<NamedEntity>();
                entityNames.Add(namedEntity);
                
                lastFoundIndex = sentence.IndexOf(entityName, endIndex, comparison);
            }

            lastEntityName = entityName;
        }

        return new SentenceEntities(sentence, entityNames ?? Array.Empty<NamedEntity>());
    }

    
}

public class SentenceEntities
{
    public SentenceEntities(string sentence, IList<NamedEntity> entities)
    {
        this.Sentence = sentence;
        this.Entities = entities;
    }

    public string Sentence { get; set; }
    public IList<NamedEntity> Entities { get; set; }

    //public void Deconstruct(out string Sentence, out IList<NamedEntity> Entities)
    //{
    //    Sentence = this.Sentence;
    //    Entities = this.Entities;
    //}
}

public class NamedEntity
{
    public NamedEntity(string name, int startIndex, int endIndex)
    {
        this.Name = name;
        this.StartIndex = startIndex;
        this.EndIndex = endIndex;
    }

    public string Name { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }

    //public void Deconstruct(out string Name, out int StartIndex, out int EndIndex)
    //{
    //    Name = this.Name;
    //    StartIndex = this.StartIndex;
    //    EndIndex = this.EndIndex;
    //}
}