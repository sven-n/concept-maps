using ConceptMaps.DummyTripleService;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TripleService>();

// Swagger/OpenAPI:
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/get-triples", async([FromBody] string text, TripleService tripleService) =>
{
    var triples = await tripleService.GetTriplesAsync(text);
    return triples;
})
.WithOpenApi();

app.Services.GetService<TripleService>()?.PrepareModel();

app.Run();
