## Crawled data
The Input for the NER Model is located here:
 `src\ConceptMaps.UI\bin\Debug\net7.0\publish\crawl-results`
 Option 1: (Funktionier momentan nicht)
 Copy the NerTrainingData into the folder "Train_NER". Example file `got_2023-06-26T18_32_42_NerTrainingData`
 Copy the SentenceRelationships into the folder "Train_NER". Example file
`got_2023-06-26T18_32_42_SentenceRelationships`

### NER Model Option 1
0. Rename the copied filed into `NerTrainingData`. Open this file and rename `training_data_crawl-results` into `training_data_got`
1. Run python file `Train_NER_Modell`
2. The trained Modell is located in th folder: `Modell_NER`
 
### NER Model Option 2 (vermutlich löschen wir diese Variante)
0. Rename the copied filed into `SentenceRelationships`. Open this file and rename `training_data_crawl-results` into `training_data_got`
1. Run python file `convert_NER.ipynb`
2. The `SentenceRelationships` is converted into `training_set`, which has the correct NER format.
3. Run python file `Train_NER_Modell`
4. The trained Modell is located in th folder: `Modell_NER`


### Relations Modell
0. Run the script `test.py` for annotation of the sentences for training purposes. (Goldenstandard: example chat GPT, alicebob.txt) The Output is `alicebob.json` 
1. Run the script `binary_converter`. The Output are three spacy files `relations_dev.spacy`, `relations_traininh.spacy`, `relations_test.spacy`
2. Change the directory to `models/relations`. Type the following command:
`spacy project run train_cpu`

