using ConceptMaps.Crawler;
using Microsoft.Extensions.DependencyInjection;

if (args.Length == 0)
{
    Console.WriteLine("The path to a configuration must be provided as first starting parameter.");
    return;
}

if (args.Length == 2)
{
    // Bei 2 Parametern nehmen wir an, dass wir bereits gecrawled haben, und nun
    // nur noch die Beziehungen analysieren wollen
    Console.WriteLine("Analyzing for relationship sentences ...");
    new RelationshipAnalyzer().AnalyzeAndStoreResults(
        textFilePath: args[0],
        relationshipFilePath: args[1],
        resultFilePath: args[0].Replace("_Text.txt", "_SentenceRelationships.json"));
    return;
}

var timestamp = DateTime.Now;

var relationshipExtractorFactory = new RelationshipExtractorFactory();
relationshipExtractorFactory.Register(new GameOfThronesRelationshipExtractor());

var configName = args[0].Split('.').First();
var fileNamePrefix = $"{configName}_{timestamp:s}".Replace(':', '_');

// Preparing dependency injection container ...
var serviceCollection = new ServiceCollection()
    .AddLogging(builder => builder
        .AddConsole()
        .AddFile($"{fileNamePrefix}_Log.txt")
        .AddFilter(level => level >= LogLevel.Information))
    .AddTransient<IWebsiteSettingsLoader, SimpleWebsiteSettingsLoader>()
    .AddSingleton(relationshipExtractorFactory)
    .AddTransient<ICrawler, Crawler>();

await using var serviceProvider = serviceCollection.BuildServiceProvider();

var settingsLoader = serviceProvider.GetRequiredService<IWebsiteSettingsLoader>();
var settings = settingsLoader.LoadSettings(args[0]);

var textFilePath = $"{fileNamePrefix}_Text.txt";
var relationshipFilePath = $"{fileNamePrefix}_Relationships.txt";
var sentencesFilePath = $"{fileNamePrefix}_SentenceRelationships.json";

// todo: Abbrechen ermöglichen.
using var cts = new CancellationTokenSource();

var crawler = serviceProvider.GetRequiredService<ICrawler>();
await crawler.CrawlAsync(settings, textFilePath, relationshipFilePath, cts.Token);

Console.WriteLine("Finished Crawling, starting analyzing for relationship sentences ...");

// After crawling analyze the sentences for possible relationships
var relationshipAnalyzer = new RelationshipAnalyzer();
relationshipAnalyzer.AnalyzeAndStoreResults(textFilePath, relationshipFilePath, sentencesFilePath);

Console.WriteLine("Finished");
