from encodings.utf_8 import decode
from http.server import BaseHTTPRequestHandler, HTTPServer
from triplegen import TripleGenerator
from convert_relations import convert
import json

hostName = "localhost"
serverPort = 5001
triples_endpoint_path = "/get-triples"
convertrel_endpoint_path ="/convert_rel"
tripleGenerator = TripleGenerator()
default_encoding = "utf-8"

class MyRequestHandler(BaseHTTPRequestHandler):
    def write_as_json_to_response(self, obj):
        # HTTP Header:
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()

        # Zu JSON serialisieren und in den response body schreiben:
        serialized = json.dumps(obj, default=vars)
        result_bytes = bytes(serialized, default_encoding)
        self.wfile.write(result_bytes)
        print("Wrote %s bytes as response" % len(result_bytes))
    def get_body_as_str(self) -> str:
        content_len = int(self.headers.get('Content-Length'))
        print("Received request with %s bytes", content_len)
        body_bytes = self.rfile.read(content_len)
        return decode(body_bytes, default_encoding)[0]

    def handle_triples_request(self):
        body_string = self.get_body_as_str()
        triples = tripleGenerator.generate(body_string)
        print("Generated %s triples" % len(triples))
        self.write_as_json_to_response(triples)

    def handle_relations_conversion(self):
        body_string = self.get_body_as_str()
        json_obj = json.loads(body_string)
        converted_sentences = convert(json_obj)
        self.write_as_json_to_response(converted_sentences)

    def do_GET(self):
        print("GET received")
        self.send_response(400)
        self.end_headers()
        self.wfile.write(bytes("Wrong HTTP Method. The service expects POST instead of GET.", default_encoding))

    def do_POST(self):
        print("POST received")
        if self.path == triples_endpoint_path:
            self.handle_triples_request()
        if self.path == convertrel_endpoint_path:
            self.handle_relations_conversion()
        else:
            self.send_response(400)
            self.end_headers()
            self.wfile.write(bytes("Wrong URI. Check console output for the expected URI", default_encoding))
            print("Wrong URI. Expected: 'http://%s:%s%s'. The text is passed in the body of the POST request." % (hostName, serverPort, triples_endpoint_path))
        

if __name__ == "__main__":        
    webServer = HTTPServer((hostName, serverPort), MyRequestHandler)
    print("Server started http://%s:%s" % (hostName, serverPort))

    print("Example request: POST 'http://%s:%s%s' with the text content written into the body with UTF-8 encoding." % (hostName, serverPort, triples_endpoint_path))

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()
    print("Server stopped.")