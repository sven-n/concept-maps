# Fachpraktikum Sprachtechnologie, Thema 'Generierung von Concept-Maps aus Texten'

Erstellt durch Korvin Felix Andreas Walter, Adriana Mikuteit und Sven Nicolai.

## Einleitung

Das Ziel dieses Projektes ist eine Web-Anwendung, welche anhand von frei eingegebenen
englischen Texten einen Familienstammbaum in Form einer Grafik generieren kann.

Hierfür wird maschinelles Lernen eingesetzt und ein Sprachmodell trainiert.
Details hierzu werden nachfolgend beschrieben.

Die Web-Anwendung unterstützt hierbei von der der Datengewinnung, bei der Aufbereitung
von Trainingsdaten sowie beim Training eines Sprachmodells selbst.
Es ist kein Programmierwissen oder explizites Wissen über die eingesetzte
Sprach-Bibliothek nötig.

Außerdem ist es möglich ein trainiertes Sprachmodell anhand weiterer Trainingsdaten
zu evaluieren.

### Warum Spezialisierung auf Familienstammbäume?

Wir haben uns für die Spezialisierung auf Familiestammbäume entschieden, und dies aus verschiedenen Gründen. Zum einen ist dieser Anwendungsfall hinreichend eng gefasst, um sich nicht in zu vielen Sonderfällen zu verstricken und der Umfang dieser Aufgabe erscheint dem zeitlichen Horizont des Fachpraktikums angemessen.

Darüber hinaus liegt der Nutzen eines derart erstellten Stammbaums auf der Hand, insbesondere wenn man den Trend der letzten Jahre in Serien, Büchern und anderen Medien zu komplexeren "Familiendrama" sieht (Beispiele hierfür sind Das Lied von Feuer und Eis, Harry Potter, ...), aber auch für Klassiker wie "Krieg und Frieden" oder "Die Buddenbrooks", in denen leicht nachvollziehbare Informationen über die Familienstrukturen hilfreich sind. Diese familiären Beziehungen sind oft schwer verständlich, wenn sie im Fließtext beschrieben werden.

Die Verwendung eines Stammbaums ermöglicht es, diese Informationen leichter zu vermitteln, indem er die familiären Beziehungen in einer übersichtlichen grafischen Form darstellt. Ein Stammbaum kann somit als eine Art Concept Map betrachtet werden. Die Beziehungen der Mitglieder zueinander sind größtenteils eindeutig zu beschreiben, sodass hier auch genügend Datengrundlage für die Erstellung von Trainingsdaten vorhanden ist.

Schließlich ist der Aufbau und die Nutzung von Stammbäumen den meisten Menschen vertraut und stellt damit einen konkreten und niederschwelligen Ansatz dar, auch für Menschen, die mit dem Konzept des Content Mapping ansonsten keine Berührungspunkte hatten. In unserem Projekt werden Concept Maps behandelt, die eine visuelle Darstellung von Ideen und Konzepten sowie ihrer Verbindungen darstellen. Die Erstellung einer Concept Map kann zeitaufwendig sein, insbesondere wenn wir eine automatisierte Erstellung planen. In diesem Fall benötigen wir im Voraus Informationen (Trainingsdaten), um eine geeignete Concept Map zu erstellen.

### Eingesetzte Technologien

Um dieses Ziel zu erreichen, setzen wir mehrere Technologien ein, u.a.:

  * Python-Backend für die sprachtechnologische Analyse der Texte, sowie des Trainings
    * __[spaCy 3.5.4](https://spacy.io/)__ als NLP-Toolkit
  * Web-Frontend auf Basis von __[ASP.NET Blazor-Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-7.0#blazor-server)__
    * __[Bootstrap 5.1](https://getbootstrap.com/)__ für ein einheitliches CSS-Styling
    * __[Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams)__ für die Anzeige eines interaktiven Graphen
    * __[AbotX](https://github.com/sjdirect/abotx)__ für das Crawling von Webseiten

#### Warum spaCy als Toolkit?

spaCy bietet vielfältige Funktionen und kann im Vergleich zu anderen Toolkits wie z.B. PyTorch und NLTK getestet, dank der umfassenden Dokumentation relativ einfach verwendet werden.
Es finden sich auch zahlreiche Beispiele, u.a. auch um Relationen zwischen Entitäten
zu ermitteln.

#### Warum über NER-Modell/Relation-Modell?


Es wurde die Idee getestet, die in Spacy implementierten Methoden anzuwenden und sich einen Parser-Tree von jedem Verwandtschaftsverhältnisse enthaltenen Satz ausgeben zu lassen. In diesem könnte man automatisch vom  Verwandtschaftsverhältnis bezeichnete Wort (z.B. „Mutter“) ausgehend die Satzstruktur durchwandern, bis man die dazugehörigen Subjekte/Objekte findet. Diese Vorgehensweise erwies sich aus mehreren Gründen als schlecht durchführbar: Zum einem hätte man eine umfassende Liste von Verwandtschaftsverhältnis bezeichneten Schlagwörtern aufsetzen müssen, die sehr große Umfänge erreichen könnte. Zum anderen war ohne „echtes  Verständnis“ des Satzes es schwierig zu identifizieren welche Subjekte/Objekte die gesuchten sind (Beispiele: „A ist die Mutter von B“ oder „A wird von seiner Mutter B gefragt“). Zudem wäre es sehr schwierig alle Formen von verneinten Verwandtschaftsverhältnissen zu unterscheiden. Daher wurde entschieden, dass auf entsprechende Aufgaben trainierte Modelle benötigt werden.


### Warum Wahl von Transformer beim Relation-Modell?

Transformer-Modell zeigen für viele Aufgaben in der NLP eine hohe Effizienz. Durch ihre Feedforward-Architektur (im Vergleich mit rückgekoppelten Modellen wie z.B. Long-Short-Term-Memory-Modells) sind sie sehr gut darin für Worte den umgebenden Kontext zu untersuchen. Dies macht sie für unsere Fragestellung die Zusammenhänge zwischen drei Wörtern (2 Entitäten, 1 Verwandtschaftsverhältnis) zu finden.


#### Eingesetzte Modelle

Für die Auswertung der vom Benutzer eingebenen Sätze kommen zwei Sprachmodelle
zum Einsatz, welche nachfolgend beschrieben werden.

##### Named Entity Recognition

Als grundlegendste Basic-Modelle werden einem in Spacy die Modelle „en_core_web_sm“ und  „en_core_web_trf“ zur Verfügung gestellt. Dabei ist das Modell „en_core_web_sm“ stärker auf Effizienz mit den zur Verfügung stehenden Rechner-Ressourcen entwickelt. Dem gegenüber ist das Modell „en_core_web_trf“ auf eine höhere Genauigkeit der Ergebnisse ausgerichtet. Beides wurde in diesem Projekt getestet.
Für die Named Entity Recognition (NER), also um die Personen innerhalb des eingegebenen
Textes zu ermitteln, setzen wir auf das bereits trainierte spaCy-Modell `en_core_web_trf`, da  die Ergebnisse  derart besser waren , dass der höhere Ressourcenaufwand als berechtigt angesehen wurde.

Wir hatten während des Projekts versucht ein eigenes Modell zu trainieren, haben
dann aber festgestellt, dass dieses Modell bereits sehr gut funktioniert.



Wir beschränken uns hierbei auf die Entitäten mit dem Tag `PERSON`.

##### Relationen

Hier sind wir auf ein Beispiel der `rel_component` von spaCy gestoßen und haben
uns daran orientiert:
https://github.com/explosion/projects/tree/v3/tutorials/rel_component

Statt der `Gold-standard REL annotations` dieses Beispiels wird das Modell mit
den Daten trainiert, welche wir mit dieser Anwendung vorbereiten.

Es kommt hierbei ein Transformer-Modell (RoBERTa) zum Einsatz, welches bereits
mit relativ wenigen Trainingsdaten gute Ergebnisse erzeugt.

Für das Training übergeben wir die Sätze jeweils als Text und den darin erwähnten
Beziehungen zwischen den Entitäten. Wir unterscheiden hierbei zwischen:
* `CHILDREN`: Annotiert eine Eltern-Kind-Beziehung.
* `SPOUSE`: Annotiert zwei Ehepartner.
* `SIBLINGS`: Annotiert Geschwister.
* `UNDEFINED`: Annotiert, dass keine Beziehung vorliegt.

Generell werden alle möglichen Kombinationen von Entitäten mit einer Beziehungsart
an das Modell übergeben, so dass es auch auf diesen Fall trainiert wird. Annotiert
der Benutzer also eine Beziehung nicht, so wird diese implizit als `UNDEFINED` annotiert.

`SPOUSE` und `SIBLINGS` werden implizit in beide Richtungen annotiert, wenn vom
Benutzer nur eine Richtung explizit annotiert wurde.

### Architektur der Lösung

![Architektur](doc/Architektur%20L%C3%B6sungsansatz.png)

## Installation

Die Anwendung läuft grundsätzlich unter Windows, Linux oder Mac OS. Wir haben dies jedoch
nur unter Windows getestet, bzw. indirekt unter Linux in Docker Containern.
Nachfolgend werden drei Wege beschrieben, wie die Anwendung in Betrieb genommen werden kann.

Da das Trainieren eines Modells sehr zeitintensiv sein kann, haben wir unser trainiertes
Modell (inkl. der Trainingsdaten), welches während der Präsentation verwendet wurde,
ebenfalls zum [Download](https://mega.nz/file/X0wQGYiK#sljp2VQfemQVp-WGKxIRoPVWb7-nEShnb0fRTAaxOMU) bereitgestellt.

### Lokal mit Hilfe von Docker

Dies ist die wahrscheinlich einfachste Möglichkeit die Anwendung zu installieren und zu starten.
Hier entfällt die Problematik, dass sich unterschiedliche Python und spaCy-Versionen
in die Quere kommen.

Hierzu sind folgende Schritte notwendig:
* Docker bzw. Docker Desktop installieren, falls noch nicht vorhanden.
* Dieses Repository lokal auf die Festplatte klonen.
* Mit der Konsole in das Unterverzeichnis `deploy` des Repositories navigieren.
* Das Kommando `docker compose up -d` ausführen
    * Die Docker-Images werden dadurch gebaut und gestartet. Dies kann einige Minuten dauern.
    * Die Anwendung ist dann unter `http://localhost:80/` verfügbar.
    * Der Port kann ggf. in der `docker-compose.yml` geändert werden bevor `docker compose` ausgeführt wird.
* Ggf. ein vortrainiertes Modell herunterladen und das Verzeichnis `training/model-best` in das Verzeichnis
  `app/training/relations/training` importieren. Dies kann z.B. über Docker Desktop folgendermaßen bewerkstelligt werden:
  * Auf den `...`-Button des Containers `concept-map-python` klicken, dann 'View files` aufrufen.
  * Nach `app/training/relations/training` navigieren, Rechtsklick -> `Import`
  * Ordner `model-best` auswählen und importieren
  * Den `concept-map-python` ggf. neu starten, wenn bereits ein anderes Modell vorhanden war.

### Lokal, mit fertigen Binaries

#### Voraussetzungen

* [.NET 7 ASP.NET Core Runtime oder SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* [Python 3.11](https://www.python.org/downloads/)
    * Installierte python packages:
        * spaCy, siehe auch [https://spacy.io/usage](https://spacy.io/usage):
            ```
            pip install -U pip setuptools wheel
            pip install -U 'spacy[transformers,lookups]=3.5.4'
            python -m spacy download en_core_web_trf
            ```
* Download des Releases: [1.0](TODO)
* Entpackter Download in ein neues Verzeichnis auf der lokalen Festplatte.
* Ggf. ein vortrainiertes Modell herunterladen und das Verzeichnis `training/model-best` in das Unterverzeichnis
  `training/relations/training` kopieren.

#### Starten

1. Doppelklick auf die Datei `python/service.py` um den Python-Service zu starten
2. Doppelklick auf `ConceptMaps.UI.exe` um die Web-Anwendung zu starten.

Die Anwendung ist dann im Browser unter `http://localhost:5000/` verfügbar.

### Lokal, selbst gebaut

#### Voraussetzungen

* Installiertes [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
  oder ein aktuelles Visual Studio 2022.
* Installiertes [Python 3.11](https://www.python.org/downloads/)
    * Installierte python packages:
        * spaCy, siehe auch [https://spacy.io/usage](https://spacy.io/usage):
            ```
            pip install -U pip setuptools wheel
            pip install -U 'spacy[transformers,lookups]=3.5.4'
            python -m spacy download en_core_web_trf
            ```

#### Bauen

Die Solution ```ConceptMaps.sln``` kann mittels .NET SDK mit dem folgendenden Konsolenbefehl gebaut werden, wenn das aktuelle Verzeichnis `src` ist:

`dotnet publish`

Alternativ kann die Solution in Visual Studio geöffnet werden und dort gebaut und
gestartet werden.

* Ggf. ein vortrainiertes Modell herunterladen und das Verzeichnis `training/model-best` in das Unterverzeichnis
  `training/relations/training` kopieren.

#### Ausführen über die Konsole

0. Sicherstellen, dass man sich mit der Konsole im Verzeichnis `src` befindet.
1. Python service mit dem folgenden Befehl starten: `python python/service.py`
2. Web-Anwendung mit dem folgenden Befehl starten: `dotnet run --project ConceptMaps.UI --urls=http://localhost:5000/`
  * Alternativ, Doppelklick auf `ConceptMaps.UI.exe` im Ordner `src\ConceptMaps.UI\bin\Debug\net7.0\publish`
  * Es ist dann im Browser unter `http://localhost:5000/` verfügbar.

## Verwendung

Nachfolgend wird die Verwendung der Anwendung beschrieben.

Allgemein wird empfohlen, dass zunächst ein einfaches Sprachmodell anhand weniger
Beispielsätze trainiert wird, damit die Analyze-Funktion verwendet werden kann.
Dies erleichtert die weitere Annotation der Sätze enorm.

Danach kann das Modell mit weiteren Trainingsdaten immer weiter verfeinert werden.

### Datenbeschaffung

Unter dem Menüpunkt "Data Retrieval" bietet die Anwendung die Möglichkeit,
vordefinierte Webseiten mit Hilfe eines Web-Crawlers zu durchforsten und daraus
Texte sowie Familienbeziehungen zu extrahieren.
Weil wir die Struktur der Fandom-Seiten kennen, können wir daraus entsprechende
Informationen extrahieren:

* ...Text.txt: Der Inhalt aller p-Elemente der gecrawlten Seiten.
* ...Relationships.txt: Die familiären Beziehungen zwischen den Personen im CSV-Format.
  * Wird anhand der Tabellen auf der rechten Seite der Fandom-Wikis ermittelt.
    Diese Daten sind oft mit Attributen wie 'data-source' annotiert, welche die
     Art der Beziehung beschreibt, sowie css-Klassen wie 'pi-data' and 'pi-data-value'.
  * Beispiel:
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
  * ..._SentenceRelationships.json: Mit den obig genannten Daten versucht der
    Crawler Sätze zu finden, in denen beide Personen einer Beziehung vorkommen.
    Er sucht hierbei die Personen sowohl mit vollem als auch mit dem Vornamen.
    Diese gefundenen Beziehungen entsprechen jedoch noch nicht zwangsläufig der
    Familienbeziehung, welche tatsächlich im Satz beschrieben ist.
    Der Benutzer hat deshalb im Nachgang die Möglichkeit die Annotation anzupassen.
  * ..._NerTrainingData.py: Der Crawler erzeugt diese Datei anhand aller Sätze
    und den Namen der bekannten Personen auf Basis der `..._Relationships.txt`-Datei.
    Es erstellt eine Python-Datei welche für das Training von NER-Modellen verwendet
    werden kann.
    Im Verlauf des Projekts haben wir jedoch festgestellt, dass das Trainieren
    eines eigenen NER-Modells nicht notwendig ist, weil das Transformer-Modell
    `en_core_web_trf` von spaCy bereits zufriedenstellend funktioniert.

#### Alternative

Die Verwendung dieser Funktion stellt jedoch nur eine Art der Beschaffung von
Trainingsdaten dar. Wie wir im Verlauf des Projektes gemerkt haben, kann es aufgrund
des Aufwands für die Datenaufbereitung Sinn machen, sich für die erste Iteration
eines Modells anderweitig Trainingssätze mit bekannten Beziehungen zu beschaffen.

Ein ziemlich guter Ansatz war es hierzu, sich mittels eines Large Language Models (LLM)
wie z.B. ChatGPT, entsprechende Sätze generieren zu lassen und mittels Batch-Funktion
der nächsten Seite "Data Preparation" zu annotieren.

Beispiel für einen Prompt:

`Bob and Alice are married. Generate 30 sentences where this relationship is included.`

Ergebnis:
```
1. Bob and Alice have been happily married for 10 years.
2. The bond between Bob and Alice grew stronger after their marriage.
...
```

### Datenaufbereitung

Diese Seite bietet vielfältige Möglichkeiten die Daten für das Training vorzubereiten.

Jeder Satz wird hierzu einzeln dargestellt und kann entsprechend annotiert werden:
* Jeder Satz kann verschiedene Status haben, nach denen auch gefiltert werden kann:
  * __Unprocessed__: Der Satz wurde von nicht bearbeitet.
  * __Analyzed__: Der Satz wurde vom Sprachmodell analysiert.
  * __Accepted__: Der Satz und die dazugehörigen Annotationen wurden vom Benutzer
                  als korrekt annotiert.
  * __Hidden__: Der Satz ist ausgeblendet.
* Es wird zu jedem Satz eine Tabelle mit Beziehungen angezeigt, welche angepasst
  werden kann, solange der Satz noch nicht im Status `Accepted` ist.
* Die Funktion __Analyze__ ermöglicht es, __wenn bereits ein Modell trainiert wurde__,
  den Satz analysieren zu lassen, d.h. es werden die Entitäten ermittelt und
  entsprechende Einträge in der Beziehungstabelle eingefügt.
  Dies erspart dem Benutzer das mühsame Eintragen der Personen in die Tabelle
  und gibt bereits einen Eindruck, wie gut (oder schlecht) das aktuelle Modell
  funktioniert.

Die Daten werden in sog. Sessions gehalten, welche dateibasiert gespeichert sowie
für späteres Weiterarbeiten geladen werden können.

Anhand einer Session kann dann ein Trainingsdatensatz erstellt werden.
Da für das Training auch mehrere Trainingsdatensätze ausgewählt werden können,
empfiehlt es sich, die Sessions nicht übermäßig groß werden zu lassen, sondern
diese sinnvoll aufzuteilen, so dass später beim Training und der Evaluation
verschiedene Kombinationen ausprobiert werden können.

Über die obere Button-Leiste stehen folgende Funktionen zur Verfügung:

* __+ Sentence__: Es wird ein neuer, leerer Satz hinzugefügt. Über den Pfeil auf
  der rechten Seite klappt sich ein Untermenü mit weiteren Möglichkeiten auf.
  * __+ Crawled__: Hier kann ein Datensatz mit den vorher gecrawlten Daten geladen werden.
    Alle gecrawlten Sätze, welche Beziehungen enthalten könnten (`..._SentenceRelationships.json`),
    werden hierzu der Session hinzugefügt.
  * __+ Batch__: Hier können mehrere Sätze mit den gleichen Beziehungen in einem Arbeitsgang
    hinzugefügt werden. Wie bereits erwähnt, ist diese Funktion hilfreich, um generierte
    Sätze eines LLM mit geringem Aufwand hinzuzufügen.

* __Data__: Hier finden sich Verwaltungsfunktionen für die Daten.
  * __Name__: Hier kann ein Name für die Session vergeben werden. Dieser wird beim
          Speichern verwendet. Standardmäßig ist dies der aktuelle Zeitstempel.
  * __Load Session__: Eine zuvor gespeicherte Session kann ausgewählt und wieder
    in die Anwendung geladen werden.
    Hinweis: Die zuvor bestehende Session wird hierbei verworfen! Etwaige Änderungen
    sollten also vorher gespeichert worden sein.
  * __Save Session__: Die Session wird mit dem vorher festgelegten Namen gespeichert.
    Hinweis: Ist bereits eine Session mit dem gleichen Namen vorhanden, so wird
    diese überschrieben!
  * __Clear Session__: Die aktuelle Session wird verworfen und eine neue wird begonnen.
  * __Upload__: Eine zuvor heruntergeladene Session kann hier wieder eingeladen werden.
  * __Download__: Eine zuvor gespeicherte Session kann hier heruntergeladen werden.
  * __Training / Save for training__: Alle Sätze mit dem Status `Accepted` werden als
    Trainingsdatensatz gespeichert, so dass er dann auf der Seite `Training`
    und `Evaluation` verfügbar ist.
  * __Training / Download__: Der zuvor gespeicherte Trainingsdatensatz kann heruntergeladen
    werden. Auf der Seite `Training` gibt es die Möglichkeit, diese Datei auch
    wieder hochzuladen.

* __Analyze all__: Mit dieser Funktion ist es möglich, für alle Sätze im Status
                   `Unprocessed` die Analyze-Funktion nacheinander auszuführen.
                   Bei großen Sessions kann dies jedoch sehr lange dauern.

### Training
In der Datenaufbereitung wurden entsprechenden Sätze gespeichert, die nun für das Training genutzt werden können. Durch die Auswahl eines Trainigssets kann das Training des Modells gestartet werden.  Dabei erfolgt die Aufteilung der Traingsdaten in 50% Train, 30% Validation und 20% Test.

* `E:` Trainingsrunde für das Modell
* `#:` Nummer des verarbeiteten Datenchargens
* `LOSS TRANSFORMER:` Ergebnis der Verlustfunktion des Transformer-Modells 
* `LOSS RELATION:` Ergebnis der Verlustfunktion in Beziehungsextraktion.
* `REL_MICRO_P:` Präzision ist ein Maß, wie viele der vom Modell als positiv vorhergesagten Fälle tatsächlich positiv sind.
* `REL_MICRO_R:` Rückruf ist ein Maß, wie viele der tatsächlich positiven Fälle vom Modell erkannt werden. 
* `REL_MICRO_F:` F-Score beschreibt einen einzelnen Wert, der Präzision und Rückruf kombiniert und die Gesamtleistung des Modells bewertet.
* `SCORE:` Gesamtbewertung der Modellleistung

Für die Verlustfunktionen gilt: Je kleiner, desto besser.
Für die anderen Parameter, wie Präzision, Rückruf, F-Score und Gesamtscore, gilt: Je größer, desto besser.



### Evaluation

Sobald ein Modell trainiert wurde, kann man das Modell gegen verschiedene Sets von Sätzen auf seine Qualität hin prüfen. Die entsprechenden Sets werden ebenfalls während der Datenaufbereitung erstellt. 

Die Ausgabe bei der Evaluation erfolgt wie folgt:
* `Gesamt:` Gesamtanzahl aller Sätze
* `Processed:` Anzahl der Sätze, gegen die evaluiert wurde
* `Correct:` Anzahl der Sätze, die durch das Modell korrekt vorhergesagt wurden
* `Failed:` Anzahl der Sätze, die durch das Modell falsch vorhergesagt wurden
* `Failed Sentences:` Ausgabe, der falsch vorhergesagten Sätze mit Begründung und dem Score der Vorhersage

Neben der reinen Qualitätskontrolle durch die Evaluation kann dieses Verfahren auch zur Modellverbesserung genutzt werden. Durch die Analyse der falsch vorhergesagten Sätze können Erkenntnisse darüber gewonnen werden, welche Aspekte das Modell noch nicht gut vorhersagen kann. Durch die Bereitstellung weiterer Trainingsdaten könnte die Qualität des Modells nachträglich verbessert werden.


### Concept Maps

Wenn wir nun ein Modell trainiert haben, können wir unsere Concept Maps in Form
von Familienstammbäumen erstellen.

## Details zur Implementierung des Python-Services

Der Quellcode zur Implementierung befindet sich unter `src/python`.
Der Einstiegspunkt ist hier die Datei `service.py`.
Es wird hier ein HTTP-Service unter dem Port 5001 gestartet, welcher die folgenden Funktionen anbietet:

#### [POST] /get-triples

Analysiert einen übergebenen Text und liefert die Beziehungen mit der entsprechenden Wahrscheinlichkeit (Score).

Der Eingabetext wird hier als reiner String im Body der Anfrage geschickt.

Das trainierte Modell analysiert den Text, und liefert die gefundenen Beziehungen.
Der Quellcode dazu findet sich in `model2.py`.
Zunächst werden dazu die Entitäten im Text mit dem NER-Modell ermittelt. Daraus
werden nur diese mit `PERSON`-Label für die weitere Analyse im Relationenmodell verwendet.

Das Relationenmodell liefert ein Dictionary, welches als Schlüssel die Entitätenpaare verwendet,
und als Wert einen Wahrscheinlichkeitswert (Score) zwischen 0 und 1 enthält.

Hier kann ein Ergebnis in zwei Richtungen entstehen, weshalb wir für ein Entitätenpaar nur das Ergebnis
in der Richtung weiterverwenden, welches den höheren Score hat.

Als endgültiges Ergebnis wird ein JSON im HTTP-Response zurückgeliefert,
welches die gefundenen Beziehungen beschreibt. Beispiel:
```json
[
    {
        "fromWord": "Thomas",
        "edgeWord": "has_child",
        "toWord": "Andreas",
        "score": 0.9999
    },
    {
        "fromWord": "Andreas",
        "edgeWord": "spouse",
        "toWord": "Marie",
        "score": 0.9998
    }
]
```

#### [POST] /training/relations/start

Hierüber kann das Training des Relationenmodells angestoßen werden.
Im Body der Anfrage werden die zu verwendenen Trainingsdaten (Sätze inkl. Beziehungen) übertragen.
Die Daten werden in einem einfachen JSON-Format übertragen, welche nur die Sätze und die Beziehungen,
aber keine Indizes o.ä. enthält.

Um dies für spaCy nutzbar zu machen, werden diese Sätze in spaCy-Docs konvertiert (s. `binary_converter.py`).
Die Web-Oberfläche liefert hier ausschließlich Daten in einem Format, welches im Code als "simple" benannt ist,
deshalb wird hier nur darauf eingegangen.

Je Satz werden zunächst alle Entitäten (Personen) anhand der mitgelieferten Beziehungen dem spaCy Doc hinzugefügt.

Dazu müssen die Tokens und deren Indizes ermittelt werden.
Die Tokenisierung wird hier mittels des Modells `en_core_web_trf` durchgeführt (`extract_entity_tokens`).

Anschließend werden die Tokens der Entitäten anhand ihrer Namen ermittelt (`parse_entities`) und paarweise dem Dictionary des
Relationenmodells mit dem Initialwert von `0.0` für alle möglichen Beziehungslabels hinzugefügt (`prepare_rels`).
Die vom Benutzer eingegebenen Beziehungen werden anschließend in dieses Dictionary eingebracht (`parse_relations_simple`).

Per Zufallsprinzip werden die Sätze dann in drei Teile (`train`, `dev`, `test`) aufgeteilt.
In `test` landen etwa 20 % und in `dev` etwa 30 % aller Sätze. Der Rest wird für `train` verwendet.
Als Ergebnis liegt dann jeweils eine spacy-Datei im Datenverzeichnis des spaCy-Projekts unter `src/python/training/relations/data`.

Das Training wird dann in einem eigenen Prozess gestartet (`train_model.py`, `start_training`).
Es wird dazu der Workflow `all` des spaCy-Projekts gestartet, welches das Training mittels CPU und eine anschließende Evaluation durchführt.

Die Konsolenausgaben und der Status dieses Prozesses werden in eigenständigen Threads überwacht (`train_model.py`, `watch_training`),
damit diese über `/training/relations/status` abgefragt werden können.

#### [POST] /training/relations/cancel

Hierüber kann ein zuvor gestarteter Trainingsprozess abgebrochen werden.

#### [GET] /training/relations/status

Liefert den aktuellen Status des Trainingsprozesses und die bisher aufgelaufenen Konsolenausgaben, falls verfügbar.

## Details zur Implementierung der Web-Oberfläche

Wie bereits weiter oben erwähnt, wurde die Oberfläche mit Hilfe von Blazor Server und .NET 7 implementiert.
Die Wahl fiel darauf, weil wir einerseits Erfahrungen damit mitbringen, und andererseits die Entwicklung
stark vereinfacht wird. Es musste z.B. kein JavaScript-Code geschrieben werden, obwohl die Oberfläche
der Anwendung sehr interaktiv ist. Die Änderungen am DOM werden bei dieser Blazor-Variante
auf dem Server berechnet und nur die Änderungen an den Client über SignalR (u.a. über WebSockets) übertragen.

Der Quellcode dazu findet sich unter `src/ConceptMaps.UI/`, und der Einstiegspunkt der Anwendung ist die `Program.cs`.
In der `Program.cs` wird die Anwendung initialisiert, d.h. nötige Services für die Dependency Injection werden registriert,
Ordner für Daten initialisiert und über den Webserver verfügbar gemacht.

Zur grundsätzlichen Struktur ist zu sagen, dass die einzelnen Seiten in dem `Pages`-Unterordner implementiert sind,
darin verwendete (wiederverwendbare) Komponenten im Ordner `Components`, und beteiligte Dienste im Ordner `Services`.

Aufgrund des Umfangs werden nachfolgend nur einige interessante Aspekte der Implementierung beschrieben.

### Diagrammerstellung

Die Anwendung erstellt anhand der vom Python gelieferten Tripel entsprechende Graphendiagramme.
Diese in einer sinnvollen Art und Weise darzustellen war ein wesentlicher Aspekt der Anwendung.

Als Komponente dient uns hier [Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams),
welche jedoch nicht das Layouting (Anordnung der Personenknoten) für uns übernimmt.

Für die Erstellung der Diagramme wurde ein eigener Service (`Services/DiagramService`) implementiert,
welcher anhand der empfangenen Tripel ein neues `Diagram`-Objekt erstellt und die Personen
und Beziehungen darin sinnvoll einfügt.

#### Anordnung der Personenknoten

Für das die Anordnung der Personen wurden verschiedene Algorithmen von
[GraphShape](https://github.com/KeRNeLith/GraphShape) ausprobiert.
Diese Algorithmen implementieren alle das generische Interface
`GraphShape.Algorithms.Layout.ILayoutAlgorithm<TVertex, TEdge, TGraph>`, wobei `TGraph` eine Implementierung
von `QuikGraph.IBidirectionalGraph<TVertex, TEdge>` sein muss. [QuckGraph](https://github.com/KeRNeLith/QuikGraph)
ist also eine weitere Bibliothek, welches die Datenstruktur für unseren Graphen definiert.

Um einen solchen Graphen zu erstellen, haben wir die Methode `DiagramExtensions.AsGraph` implementiert,
welche als Input das (unangeordnete) `Blazor.Diagrams.Diagram` entgegennimmt, und daraus
den nötigen bidirektionalen Graphen von `QuikGraph` erstellt.

Im Verlauf des Projektes haben wir jedoch festgestellt, dass die vorhandenen Algorithmen
jedoch nicht das gewünschte Ergebnis als Stammbaum liefern können.
Wir haben uns deshalb entschieden, eine eigene Implementierung für das Interface `GraphShape.Algorithms.Layout.ILayoutAlgorithm<TVertex, TEdge, TGraph>`
zu schaffen. Diese lehnt sich an `GraphShape.Algorithms.Layout.SimpleTreeLayoutAlgorithm<TVertex, TEdge, TGraph>` an,
wurde jedoch speziell für Familenstammbäume angepasst. Sie ist in der Klasse `FamilyTreeLayoutAlgorithm` zu finden.

Der Algorithmus geht folgendermaßen vor (s. `FamilyTreeLayoutAlgorithm.InternalCompute`):
  * Es werden zunächst alle Personen ohne Eltern ermittelt und anhand der Anzahl der nachkommenden Generationen sortiert.
    Für jede Generation wird ein eigenes Set mit darin enthaltenen Eltern erstellt.
  * Nun werden alle Kinder der Eltern entsprechend in der Liste der darunterliegenden Generation einsortiert
  * Nun werden die Ehepartner der Personen in der Liste in die jeweils gleiche Generation einsortiert
  * Ggf. übrige Personen werden in einer weiteren, darunterliegenden Generation hinzugefügt

Als Ergebnis kommt daraus eine Liste mit Generationen-Sets heraus, wobei im ersten Set die Familienoberhäupter zu finden sind.
Anhand dieser Liste wird nun Generation für Generation der Graph aufgebaut, und die Koordinaten der einzelnen Knoten berechnet.

Am Ende werden die berechneten Positionen auf die entsprechenden Knoten des `Blazor.Diagrams.Diagram` zugewiesen.

#### Vereinfachung der Darstellung der Beziehungen

Damit der Stammbaum nicht zu überladen dargestellt wird, werden nach der Anordnung der Knoten einzelne Kanten
des Graphen aus dem fertigen Diagramm entfernt:
  * Beziehungen zwischen Geschwistern, welche die gleichen Eltern haben.
  * Die Beschriftung auf der Kante von Eltern zu Kindern, da die Beziehung durch einen Pfeil dargestellt wird

Ansonsten werden Beziehungen als Kanten wie folgt dargestellt:
  * Ehepartner mit einer geraden horizontalen Linie mit einem Ring-Icon 💍
  * Geschwungene Linie mit Pfeil zum Kind
  * Geschwister untereinander mit unterschiedlichen oder unbekannten Eltern mit einer grauen Linie

### Datenablage

Daten werden in unterschiedlichen Unterverzeichnissen des Working-Directories (gewöhnlicherweise der Ort des Anwendungsstarts) abgelegt:

* `crawler-settings`: Die Einstellungen für den Crawler je Webseite.
* `crawl-results`: Die Ergebnisse des Web-Crawlers.
* `prepare-data`: Die Sessions der Data-Preparation-Seite.
* `training-data`: Die vorbereiteten Trainingsdaten, welche u.a. auch auf der Training-Seite angezeigt werden.

### Konsolenausgabe bei Training des Modells

Die Trainingsseite des Modells ruft periodisch den Status vom Python-Service (s. `[GET] /training/relations/status`) ab.
Die Konsolenausgabe davon enthält Formatierungscodes, welche von der Webanwendung entsprechend mittels CSS sichtbar umgesetzt werden.

Die Interpretierung dieser Codes wurde in der Komponente `ConsoleText` implementiert, s. `Components\ConsoleText.razor.cs`.

## Limitierungen der aktuellen Implementierung und Ausblick

* Die URLs zum Python-Service sind in der Web-Anwendung noch festgelegt auf
 `localhost:5000`. Es würde Sinn machen, dies konfigurierbar zu machen.
* Der Crawler kann nicht zuverlässig abgebrochen werden, s. https://github.com/sjdirect/abot/issues/240.
* Aktuell kann nur ein Relationenmodell gleichzeitig existieren. Das Training überschreibt
  immer das vorherige. Sinnvoll wäre es, wenn mehrere Modelle zeitgleich vorhanden
  sein könnten, und diese vom Benutzer auch wieder geladen und verglichen werden könnten.
* Das Training des Relationenmodells fängt immer wieder bei null an. Daher wäre
  es vorteilhaft, wenn ein vorhandenes Modell durch neue Trainingsdaten weiter
  trainiert werden könnte.
* Es wäre zu überlegen, das Relationenmodell auf das spaCy Modell `en_core_web_trf`
  aufbauen zu lassen, so dass es nicht mehr notwendig ist, ein separates Modell für
  das NER laden zu müssen. Somit würde der Python-Service ressourcenschonender agieren.
* Auflösung von Cross-References im Eingabetext, bevor die Relationen ermittelt werden.
  Beispiel:
  * Vorher:  `Bob is the father of Alice. He is also the father of Jeff.`
  * Nachher: `Bob is the father of Alice. Bob is also the father of Jeff.`
