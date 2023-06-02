from encodings.utf_8 import decode
from http.server import BaseHTTPRequestHandler, HTTPServer
from triplegen import TripleGenerator
import json

hostName = "localhost"
serverPort = 5000
endpoint_path = "/get-triples"
tripleGenerator = TripleGenerator()
default_encoding = "utf-8"

class MyRequestHandler(BaseHTTPRequestHandler):
    
    def handle_triples_request(self):
        # Body mit dem Text einlesen:
        content_len = int(self.headers.get('Content-Length'))
        print("Received request with %s bytes", content_len)
        body_bytes = self.rfile.read(content_len)
        body_string = decode(body_bytes, default_encoding)

        # Tripel erzeugen:
        triples = tripleGenerator.generate(body_string)
        print("Generated %s triples" % len(triples))

        # HTTP Header:
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()
        
        # Tripel als JSON in den Body schreiben:
        triples_serialized = json.dumps(triples, default=vars)
        result_bytes = bytes(triples_serialized, default_encoding)
        self.wfile.write(result_bytes)
        print("Wrote %s bytes as response" % len(result_bytes))

    def do_GET(self):
        print("GET received")
        self.send_response(400)
        self.end_headers()
        self.wfile.write(bytes("Wrong HTTP Method. The service expects POST instead of GET.", default_encoding))

    def do_POST(self):
        print("POST received")
        if self.path == endpoint_path:
             self.handle_triples_request()
        else:
            self.send_response(400)
            self.end_headers()
            self.wfile.write(bytes("Wrong URI. Check console output for the expected URI", default_encoding))
            print("Wrong URI. Expected: 'http://%s:%s%s'. The text is passed in the body of the POST request." % (hostName, serverPort, endpoint_path))
        

if __name__ == "__main__":        
    webServer = HTTPServer((hostName, serverPort), MyRequestHandler)
    print("Server started http://%s:%s" % (hostName, serverPort))

    print("Example request: POST 'http://%s:%s%s' with the text content written into the body with UTF-8 encoding." % (hostName, serverPort, endpoint_path))

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()
    print("Server stopped.")