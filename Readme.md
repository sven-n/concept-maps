﻿# Fachpraktikum Sprachtechnologie, Thema 'Generierung von Concept-Maps aus Texten'

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

Um dieses Ziel zu erreichen, setzen wir mehrere Technologien ein:

  * Python-Backend für die sprachtechnologische Analyse der Texte, sowie des Trainings
    * __spaCy__ als NLP-Toolkit
  * Web-Frontend auf Basis von __ASP.NET Blazor-Server__
    * __Bootstrap__ für ein einheitliches CSS-Styling
    * __Blazor.Diagrams__ für die Anzeige eines interaktiven Graphen
    * __AbotX__ für das Crawling von Daten

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

TODO

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

TODO

## Details zur Implementierung der Web-Oberfläche

TODO
