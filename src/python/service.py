"""
Implements a simple HTTP service which provides NLP (spaCy) functions
which can be called by our user interface.
"""

import json
import urllib3
from encodings.utf_8 import decode
from http.server import BaseHTTPRequestHandler, HTTPServer
from triplegen import TripleGenerator
from convert_relations import convert
from train_model import RelationModelTraining, NrtModelTraining, ModelTrainingBase

DEFAULT_ENCODING = "utf-8"
HOST_NAME = "localhost"
HOST_PORT = 5001
TRIPLES_ENDPOINT_PATH = "/get-triples"
CONVERT_REL_ENDPOINT_PATH ="/convert_rel"
TRAINING_RELATIONS_STATUS_PATH = "/training/relations/status"
TRAINING_RELATIONS_START_PATH = "/training/relations/start"
TRAINING_RELATIONS_CANCEL_PATH = "/training/relations/cancel"
TRAINING_NRT_STATUS_PATH = "/training/nrt/status"
TRAINING_NRT_START_PATH = "/training/nrt/start"
TRAINING_NRT_CANCEL_PATH = "/training/nrt/cancel"

tripleGenerator = TripleGenerator()
relationModelTrainer = RelationModelTraining()
nrtModelTrainer = NrtModelTraining()

class MyRequestHandler(BaseHTTPRequestHandler):
    """The request handler for the service."""

    def write_as_json_to_response(self, obj):
        """Write the given object as json into the response body."""

        # HTTP Header:
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()

        # Zu JSON serialisieren und in den response body schreiben:
        try:
            serialized = json.dumps(obj, default=vars)
            result_bytes = bytes(serialized, DEFAULT_ENCODING)
            self.wfile.write(result_bytes)
            print(f"Wrote {result_bytes} bytes as response.")
        except Exception as ex:
            print(ex)

    def get_body_as_str(self) -> str:
        """Get the request body as string.
        
        :return: The request body as string."""
        content_len = int(self.headers.get('Content-Length'))
        print(f"Received request with {content_len} bytes")
        body_bytes = self.rfile.read(content_len)
        return decode(body_bytes, DEFAULT_ENCODING)[0]

    def handle_triples_request(self):
        """Handle the triples generation request."""

        body_string = self.get_body_as_str()
        triples = tripleGenerator.generate(body_string)
        print(f"Generated {len(triples)} triples")
        self.write_as_json_to_response(triples)

    def handle_relations_conversion(self):
        """Handle the conversion of the relations json crawling data to the training data."""

        body_string = self.get_body_as_str()
        json_obj = json.loads(body_string)
        converted_sentences = convert(json_obj)
        self.write_as_json_to_response(converted_sentences)

    def handle_train_start(self, trainer: ModelTrainingBase):
        """Handle starting of the training"""

        if trainer.get_status().is_active:
            self.send_response(500)
            self.end_headers()
            self.wfile.write(bytes("Training is already in progress.", DEFAULT_ENCODING))
        else:
            try:
                body_string = self.get_body_as_str()
                json_obj = json.loads(body_string)
                model_source_name = self.path.replace(TRAINING_RELATIONS_START_PATH, '', 1).split('/')[0]
                trainer.start_training(json_obj, model_source_name)
                self.send_response(200)
            except Exception as ex:
                self.wfile.write(bytes(ex, DEFAULT_ENCODING))
                self.send_response(500)
            self.end_headers()

    def handle_train_cancel(self, trainer: ModelTrainingBase):
        """Handle training cancellation."""
        if not trainer.get_status().is_active:
            self.send_response(500)
            self.end_headers()
            self.wfile.write(bytes("Training is not in progress.", DEFAULT_ENCODING))
        else:
            trainer.cancel_training()
            self.send_response(200)
            self.end_headers()

    def handle_train_status(self, trainer: ModelTrainingBase):
        """Handle the training status request."""
        train_status = trainer.get_status()
        self.write_as_json_to_response(train_status)

    def do_GET(self): # pylint: disable=invalid-name
        """Handle the HTTP GET request."""
        print("GET received")
        if self.path == TRAINING_RELATIONS_STATUS_PATH:
            self.handle_train_status(relationModelTrainer)
        elif self.path == TRAINING_NRT_STATUS_PATH:
            self.handle_train_status(nrtModelTrainer)
        else:
            self.send_response(404)
            self.end_headers()
            self.wfile.write(bytes(
                "There is nothing at this address.",
                DEFAULT_ENCODING))

    def do_POST(self): # pylint: disable=invalid-name
        """Handle the HTTP POST request."""
        print("POST received")
        if self.path == TRIPLES_ENDPOINT_PATH:
            self.handle_triples_request()
        #elif self.path == CONVERT_REL_ENDPOINT_PATH:
        #    self.handle_relations_conversion()
        elif self.path.startswith(TRAINING_RELATIONS_START_PATH):
            self.handle_train_start(relationModelTrainer)
        elif self.path.startswith(TRAINING_NRT_START_PATH):
            self.handle_train_start(nrtModelTrainer)
        elif self.path == TRAINING_RELATIONS_CANCEL_PATH:
            self.handle_train_cancel(relationModelTrainer)
        elif self.path == TRAINING_NRT_CANCEL_PATH:
            self.handle_train_cancel(nrtModelTrainer)
        else:
            self.send_response(400)
            self.end_headers()
            self.wfile.write(bytes(
                "Wrong URI. Check console output for the expected URI",
                DEFAULT_ENCODING))
            print(f"Wrong URI. Expected: 'http://{HOST_NAME}:{HOST_PORT}{TRIPLES_ENDPOINT_PATH}'. \
                    The text is passed in the body of the POST request.")


if __name__ == "__main__":
    webServer = HTTPServer((HOST_NAME, HOST_PORT), MyRequestHandler)
    print(f"Server started http://{HOST_NAME}:{HOST_PORT}")

    print(f"Example request: POST 'http://{HOST_NAME}:{HOST_PORT}{TRIPLES_ENDPOINT_PATH}' \
          with the text content written into the body with UTF-8 encoding.")

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()
    print("Server stopped.")
