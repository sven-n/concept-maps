"""Functions to train the relation model."""

import subprocess

class TrainingStatus:
    """Status of the model training."""

    def __init__(self, is_active: bool, state: str, output: str, error: str):
        self.is_active = is_active
        self.state = state
        self.output = output
        self.error = error

class RelationModelTraining:
    """Class which keeps track of the model training."""

    def __init__(self):
        # Initialization of this instance.
        self.training_process: subprocess.Popen = None

    def start_training(self, training_data: str):
        """Starts the training of the relation model with the specified training data
        by starting a new process.
        
        :param training_data: The training data as string in json format.
        """

        working_dir = '../../Relationsmodell/'

        # todo: ggf. Backup von vorhandenen Modellen

        subprocess.run(['spacy', 'project', 'run', 'clean'], cwd = working_dir, check=False)

        # todo: convert training data

        # todo: set up project file accordingly

        self.training_process = subprocess.Popen(
            ['spacy', 'project', 'run', 'all'],
            cwd = working_dir,
            stdout = subprocess.PIPE,
            stderr = subprocess.PIPE)

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
        process = self.training_process
        if process is None:
            return TrainingStatus(False, 'inactive', '', '')

        # todo: p.communicate required?
        output = process.stdout.read()
        error = process.stderr.read()
        returncode = process.poll()
        if returncode is None:
            return TrainingStatus(True, 'training', output, error)

        state = 'success'
        if returncode != 0:
            state = 'failure'
        return TrainingStatus(False, state, output, error)
