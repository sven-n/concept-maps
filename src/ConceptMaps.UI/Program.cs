using ConceptMaps.Crawler;
using ConceptMaps.UI.Data;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<RemoteTripleService>();
builder.Services.AddSingleton<DiagramService>();
builder.Services.AddSingleton<ILayoutAlgorithmFactory, StandardLayoutAlgorithmFactory>();
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

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(crawlResultsFolder),
    RequestPath = "/crawl-results",
});

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
