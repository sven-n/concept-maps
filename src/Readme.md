## How to run locally

### Prerequisites

* Installed [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* Installed [Python 3.11](https://www.python.org/downloads/)
    * Installed python packages:
        * spaCy, see also [https://spacy.io/usage](https://spacy.io/usage):
            ```
            pip install -U pip setuptools wheel
            pip install -U spacy
            python -m spacy download en_core_web_sm
            ```

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

## Nice to know

### HTTP request url

The web application is requesting the url `http://localhost:5001/get-triples` with the input text in the body.

The python service is then analyzing the input text and returns the triples as json in the body as well.

All texts are encoded as `UTF-8`.

### JSON format between ```ConceptMaps.UI``` and python service

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

### UI Components

* UI Technology: [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-7.0#blazor-server)
* CSS Styling: [Bootstrap 5.1](https://getbootstrap.com/)
* Graph rendering: [Z.Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams)
* Graph layouting: [GraphShape](https://github.com/KeRNeLith/GraphShape)

### Crawler

The crawler is able to crawl pre-configured websites. It extracts informations about
personal relationships as well, if the site follows a known structure.

Fan-Websites about series, such as *Game of Thrones* are a good source for text
and their corresponding information about personal relationships
because they offer both in a structured, annotated way.

#### Output data
* ...Text.txt: The crawled text is taken from the content of all HTML p elements of a page.
* ...Relationships.txt: The relationships between the persons in a csv-format.
  * It's extracted from the information tables on the right side of a page.
     The data is often annotated with attributes like 'data-source' which contains
     the kind of relationships, and classes like 'pi-data' and 'pi-data-value'.
  * Example:
```html
<div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="Father">
    <h3 class="pi-data-label pi-secondary-font">Father</h3>
  <div class="pi-data-value pi-font">{<a href="/wiki/Hoster_Tully" title="Hoster Tully">Hoster Tully</a>}<sup id="cite_ref-GoT_104_1-2" class="reference"><a href="#cite_note-GoT_104-1">[1]</a></sup></div>
</div>

<div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="Mother">
    <h3 class="pi-data-label pi-secondary-font">Mother</h3>
  <div class="pi-data-value pi-font">{<a href="/wiki/Minisa_Tully" title="Minisa Tully">Minisa Whent</a>}<sup id="cite_ref-H&amp;L_313_6-0" class="reference"><a href="#cite_note-H&amp;L_313-6">[4]</a></sup></div>
</div>

<div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="Spouse">
    <h3 class="pi-data-label pi-secondary-font">Spouse(s)</h3>
  <div class="pi-data-value pi-font">{<a href="/wiki/Eddard_Stark" title="Eddard Stark">Eddard Stark</a>}<sup id="cite_ref-GoT_101_5-4" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup></div>
</div>

<div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="Children">
    <h3 class="pi-data-label pi-secondary-font">Child(ren)</h3>
  <div class="pi-data-value pi-font">{<a href="/wiki/Robb_Stark" title="Robb Stark">Robb Stark</a>}<sup id="cite_ref-GoT_101_5-5" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup><br><a href="/wiki/Sansa_Stark" title="Sansa Stark">Sansa Stark</a><sup id="cite_ref-GoT_101_5-6" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup><br><a href="/wiki/Arya_Stark" title="Arya Stark">Arya Stark</a><sup id="cite_ref-GoT_101_5-7" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup><br><a href="/wiki/Bran_Stark" title="Bran Stark">Bran I the Broken</a><sup id="cite_ref-GoT_101_5-8" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup><br>{<a href="/wiki/Rickon_Stark" title="Rickon Stark">Rickon Stark</a>}<sup id="cite_ref-GoT_101_5-9" class="reference"><a href="#cite_note-GoT_101-5">[3]</a></sup></div>
</div>

<div class="pi-item pi-data pi-item-spacing pi-border-color" data-source="Siblings">
    <h3 class="pi-data-label pi-secondary-font">Sibling(s)</h3>
  <div class="pi-data-value pi-font">{<a href="/wiki/Lysa_Arryn" title="Lysa Arryn">Lysa Arryn</a>}<sup id="cite_ref-GoT_105_7-0" class="reference"><a href="#cite_note-GoT_105-7">[5]</a></sup><br><a href="/wiki/Edmure_Tully" title="Edmure Tully">Edmure Tully</a><sup id="cite_ref-GoT_303_8-0" class="reference"><a href="#cite_note-GoT_303-8">[6]</a></sup></div>
</div>
```
  * ..._SentenceRelationships.json: With the data of the above files, the
    crawler tries to find sentences where both persons of a relationship occur.
    It searches for full names and first names.
  * ..._NerTrainingData.py: The crawler creates this file which contains sentences and
    the containing persons names. Based on all names in _Relationships.txt, it searches
    for occurrences in each sentence of the text.
