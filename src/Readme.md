## How to run locally

### Prerequisites

* Installed [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* Installed [Python 3.11](https://www.python.org/downloads/)
    * Installed python packages:
        * spaCy, see also [https://spacy.io/usage](https://spacy.io/usage):
            `pip install -U pip setuptools wheel`
            `pip install -U spacy`
            `python -m spacy download en_core_web_sm`

### Building

The ```ConceptMaps``` solution can be built using the .NET SDK with the following command when the current directory is `src`:
`dotnet publish`

### Running

0. Ensure, that you're in the `src` folder.
1. Start the python service for triple generation: `python python/service.py`
  * It's then available at `http://localhost:5001/get-triples`
2. Start the web application: `dotnet run --project ConceptMaps.UI --urls=http://localhost:5000/`
  * Alternatively, double click on the `ConceptMaps.UI.exe` in the folder `src\ConceptMaps.UI\bin\Debug\net7.0\publish`
  * It's then available in the Browser at `http://localhost:5000/`

### Nice to know

## HTTP request url

The web application is requesting the url `http://localhost:5001/get-triples` with the input text in the body.

The python service is then analyzing the input text and returns the triples as json in the body as well.

All texts are encoded as `UTF-8`.

## JSON format between ```ConceptMaps.UI``` and python service

Example, when `Thomas` has a child `Andreas` which is married with `Marie`:

```json
[
    {
        "fromWord": "Thomas",
        "edgeWord": "has_child",
        "toWord": "Andreas"
    },
    {
        "fromWord": "Andreas",
        "edgeWord": "spouse",
        "toWord": "Marie"
    }
]
```