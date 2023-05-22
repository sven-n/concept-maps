using ConceptMaps.Crawler;
using Microsoft.Extensions.DependencyInjection;

if (args.Length == 0)
{
    Console.WriteLine("The path to a configuration must be provided as first starting parameter.");
}

var timestamp = DateTime.Now;

var relationshipExtractorFactory = new RelationshipExtractorFactory();
relationshipExtractorFactory.Register(new GameOfThronesRelationshipExtractor());

// Preparing dependency injection container ...
var serviceCollection = new ServiceCollection()
    .AddLogging(builder => builder
        .AddConsole()
        .AddFile($"{timestamp:s}_Log.txt".Replace(':', '_'))
        .AddFilter(level => level >= LogLevel.Information))
    .AddTransient<IWebsiteSettingsLoader, SimpleWebsiteSettingsLoader>()
    .AddSingleton(relationshipExtractorFactory)
    .AddTransient<ICrawler, Crawler>();

await using var serviceProvider = serviceCollection.BuildServiceProvider();

var settingsLoader = serviceProvider.GetRequiredService<IWebsiteSettingsLoader>();
var settings = settingsLoader.LoadSettings(args[0]);

await using var contentWriter = File.CreateText($"{timestamp:s}_Text.txt".Replace(':','_'));
await using var relationsWriter = File.CreateText($"{timestamp:s}_Relationships.txt".Replace(':', '_'));

// todo: Abbrechen ermöglichen.
using var cts = new CancellationTokenSource();

var crawler = serviceProvider.GetRequiredService<ICrawler>();
await crawler.CrawlAsync(settings, contentWriter, relationsWriter, cts.Token);

Console.WriteLine("Finished");

