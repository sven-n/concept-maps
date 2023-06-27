import json
import os
from pathlib import Path
from train_model import RelationModelTraining

model_trainer = RelationModelTraining()
cwd = Path(os.getcwd())
json_file_path = cwd.joinpath('../../training-data/alicebob.json').resolve()
with open(json_file_path, encoding='utf-8') as file:
    training_data = file.read()

model_trainer.start_training(training_data)
