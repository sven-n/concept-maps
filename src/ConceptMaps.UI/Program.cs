using ConceptMaps.Crawler;
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
builder.Services.AddSingleton<ITrainingDataManager, TrainingDataManager>();
builder.Services.AddSingleton<IPrepareDataManager, PrepareDataManager>();
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

var crawlResultsFolder = Path.Combine(Environment.CurrentDirectory, CrawledDataProvider.SubFolder);
if (!Path.Exists(crawlResultsFolder))
{
    Directory.CreateDirectory(crawlResultsFolder);
}

var trainingDataFolder = Path.Combine(Environment.CurrentDirectory, TrainingDataManager.SubFolder);
if (!Path.Exists(trainingDataFolder))
{
    Directory.CreateDirectory(trainingDataFolder);
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(crawlResultsFolder),
    RequestPath = $"/{CrawledDataProvider.SubFolder}",
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(trainingDataFolder),
    RequestPath = $"/{TrainingDataManager.SubFolder}",
});

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
