## Current state

This project is under development. The code is not polished yet ;-)

I added a DummyTripleService to generate triples out of simple text.
This will be replaced later by a more advanced Python service.

## How to run

First, build the entire solution with Visual Studio 2022 or the dotnet command line interface of .NET 7 (SDK).

For the UI to work, you'd need the DummyTripleService running at port 5000.
The UI requests the text triples from ```http://localhost:5000/get-triples``` with
the text in the http request body. You can find the code [here](ConceptMaps.UI\Data\RemoteTripleService.cs).
For testing, simple double click the built exe file (see folder ```ConceptMaps.DummyTripleService/bin/Debug/```).

When the service runs, you can run ```ConceptMaps.UI``` and run your tests.
