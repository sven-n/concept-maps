"""Functions to train the relation model."""

from pathlib import Path
import os
import re
import subprocess
import sys
from threading import Thread

import binary_converter

TRAINING_CONFIG_PATH = 'configs/rel_trf.cfg'
DEFAULT_ENCODING = 'utf-8'

class TrainingStatus:
    """Status of the model training."""

    def __init__(self, is_active: bool, state: str, output: str, error: str):
        self.is_active = is_active
        self.state = state
        self.output = output
        self.error = error
        

class ModelTrainingBase:
    """Class which keeps track of the model training."""

    def __init__(self):
        # Initialization of this instance.
        self.training_process: subprocess.Popen = None
        self.read_output_thread: Thread = None
        self.read_error_thread: Thread = None
        self.output = []
        self.error = []

    def get_model_type(self):
        """Returns the model type which is handled by this training class."""
        return None

    def convert_training_data(self, training_data: list[dict], working_dir: Path):
        # to be overwritten
        raise NotImplementedError("convert_training_data must be overwritten")

    def start_training(self, training_data: list[dict], target: str, source: (str|None) = None):
        """Starts the training of the relation model with the specified training data
        by starting a new process.
        
        :param training_data: The training data as string in json format.
        :param source: The name of the source model, if an existing model should be improved.
        """

        cwd = Path(os.getcwd())
        working_dir = cwd.joinpath('training').joinpath(self.get_model_type()).resolve()

        # subprocess.run(['spacy', 'project', 'run', 'clean'], cwd = working_dir, check=False)
        self.convert_training_data(training_data, working_dir)

        args = ['spacy', 'project', 'run', 'all']

        # todo: source and target models
        # args = ['spacy', 'project', 'run', 'all', f"--vars.target_model_name={target}"]
        # config_path = working_dir.joinpath(TRAINING_CONFIG_PATH)
        # todo self.set_model_source(config_path, source)
        # if (source is not None):
            #args.append(f"--vars.source_model_name={source}")

        self.output.clear()
        self.error.clear()
        self.training_process = subprocess.Popen(
            args,
            cwd = working_dir,
            stdout = subprocess.PIPE,
            stderr = subprocess.PIPE)
        self.read_output_thread = Thread(
            target = self.watch_training,
            args = [self.training_process, self.training_process.stdout, self.output],
            name='reading training output')
        self.read_output_thread.start()

        self.read_error_thread = Thread(
            target = self.watch_training,
            args = [self.training_process, self.training_process.stderr, self.error],
            name='reading training error')
        self.read_error_thread.start()

    def watch_training(self, training_process: subprocess.Popen, source_io, targetlist: list[str]):
        while not source_io.closed:
            try:
                line = str(source_io.readline(), encoding='utf-8')
                if len(line) > 0:
                    self.output.append(line)
                if training_process.poll() is not None:
                    break
            except:
                break

    def set_model_source(self, config_path, source: (str|None)):
        """Sets the source model in the components of the model."""

        with open(config_path,
                  encoding=DEFAULT_ENCODING) as in_file:
            content = in_file.read()
            if 'source =' not in content:
                return

        replacement = "source = null\n"
        if source is not None:
            replacement = f"source = \"{source}\"\n"

        with open(config_path, 'w',
                  encoding = DEFAULT_ENCODING) as out_file:
            result = re.sub("source = (.+)?\\n", replacement, content)
            out_file.write(result)

    def cancel_training(self):
        """Cancels the training process."""
        process = self.training_process
        if process is None:
            return
        process.terminate()
        self.training_process = None

    def get_status(self) -> TrainingStatus:
        """Get the status of the training process

        :return: The status of the training process
        """
        output = "".join(self.output)
        error = "".join(self.error)
        process = self.training_process
        if process is None:
            return TrainingStatus(False, 'inactive', output, error)

        #if process.stdout is not None and not process.stdout.closed:
            #(output, error) = process.communicate(timeout=1)
            
            #self.output.append(process.stdout.readlines())
            #self.error.append(process.stderr.readline())

        returncode = process.poll()
        if returncode is None:
            return TrainingStatus(True, 'training', output, error)

        state = 'success'
        if returncode != 0:
            state = 'failure'
        return TrainingStatus(False, state, output, error)

class RelationModelTraining(ModelTrainingBase):
    """Class which keeps track of the relation model training."""

    def get_model_type(self):
        return 'relations'

    def convert_training_data(self, training_data: list[dict], working_dir: Path):
        binary_converter.create_relation_training_files(training_data, working_dir.joinpath('data'))


class NrtModelTraining(ModelTrainingBase):
    """Class which keeps track of the NRT model training."""

    def get_model_type(self):
        return 'nrt'

    def data_conversion(self, training_data: list[dict]):
        # to be implemented ...
        raise NotImplementedError()
