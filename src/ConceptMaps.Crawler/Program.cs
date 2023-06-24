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
    // nur noch die Daten aufbereiten wollen
    Console.WriteLine("Analyzing for relationship sentences ...");
    var textFilePath = args[0];
    var relationshipFilePath = args[1];
    var sentenceRelationshipsResultFile = args[0].Replace("_Text.txt", "_SentenceRelationships.json");
    new RelationshipAnalyzer(relationshipFilePath).AnalyzeAndStoreResults(
        textFilePath,
        resultFilePath: sentenceRelationshipsResultFile);

    Console.WriteLine("Generating NER training data ...");
    new NerTrainingDataGenerator(relationshipFilePath).GenerateTrainingDataFile(textFilePath);
}
else
{
    await DoFullCrawlAsync();
}

async Task DoFullCrawlAsync()
{
    var timestamp = DateTime.Now;

    var configName = args[0].Split('.').First();
    var fileNamePrefix = $"{configName}_{timestamp:s}".Replace(':', '_');

    // Preparing dependency injection container ...
    var serviceCollection = new ServiceCollection()
        .AddLogging(builder => builder
            .AddConsole()
            .AddFile($"{fileNamePrefix}_Log.txt")
            .AddFilter(level => level >= LogLevel.Information))
        .AddCrawler();

    await using var serviceProvider = serviceCollection.BuildServiceProvider();

    var settingsLoader = serviceProvider.GetRequiredService<IWebsiteSettingsProvider>();
    var settings = settingsLoader.LoadSettings(args[0]);

    var textFilePath = $"{fileNamePrefix}_Text.txt";
    var relationshipFilePath = $"{fileNamePrefix}_Relationships.txt";
    var sentencesFilePath = $"{fileNamePrefix}_SentenceRelationships.json";

    // todo: Abbrechen über die Konsole ermöglichen.
    using var cts = new CancellationTokenSource();

    var crawler = serviceProvider.GetRequiredService<ICrawler>();
    await crawler.CrawlAsync(settings, textFilePath, relationshipFilePath, null, cts.Token);

    Console.WriteLine("Finished Crawling, starting analyzing for relationship sentences ...");

    // After crawling analyze the sentences for possible relationships
    var relationshipAnalyzer = new RelationshipAnalyzer(relationshipFilePath);
    relationshipAnalyzer.AnalyzeAndStoreResults(textFilePath, sentencesFilePath);

    Console.WriteLine("Generating NER training data ...");
    var trainingDataGenerator = new NerTrainingDataGenerator(relationshipFilePath);
    trainingDataGenerator.GenerateTrainingDataFile(textFilePath);

    Console.WriteLine("Finished");
}