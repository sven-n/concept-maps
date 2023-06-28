using ConceptMaps.Crawler;
using ConceptMaps.UI.Pages;
using ConceptMaps.UI.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<SentenceAnalyzer>();
builder.Services.AddSingleton<RemoteTripleService>();
builder.Services.AddSingleton<RemoteTrainingService>();
builder.Services.AddSingleton<IModelProvider, ModelProvider>();
builder.Services.AddSingleton<ITrainingDataProvider, TrainingDataProvider>();
builder.Services.AddSingleton<DiagramService>();
builder.Services.AddSingleton<ILayoutAlgorithmFactory, StandardLayoutAlgorithmFactory>();
builder.Services.AddSingleton<ICrawledDataProvider, CrawledDataProvider>();
builder.Services.AddCrawler();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

var crawlResultsFolder = Path.Combine(Environment.CurrentDirectory, "crawl-results");
if (!Path.Exists(crawlResultsFolder))
{
    Directory.CreateDirectory(crawlResultsFolder);
}

var trainingDataFolder = Path.Combine(Environment.CurrentDirectory, "training-data");
if (!Path.Exists(trainingDataFolder))
{
    Directory.CreateDirectory(trainingDataFolder);
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(crawlResultsFolder),
    RequestPath = "/crawl-results",
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(trainingDataFolder),
    RequestPath = "/training-data",
});

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
