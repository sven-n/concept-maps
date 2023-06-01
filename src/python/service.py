from encodings.utf_8 import decode
from http.server import BaseHTTPRequestHandler, HTTPServer
from triplegen import TripleGenerator
import json

hostName = "localhost"
serverPort = 5000
tripleGenerator = TripleGenerator()

class MyRequestHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        print("GET received")
    def do_POST(self):
        print("POST received")
        self.send_response(501)
        self.end_headers()
        return
        
        if self.path == '/get-triples':

            # Body mit dem Text einlesen:
            content_len = int(self.headers.get('Content-Length'))
            print("Received request with %s bytes", content_len)
            self.headers.get('Content-Length')
            body_bytes = self.rfile.read(content_len)
            body_string = decode(body_bytes, "utf-8")

            # Tripel erzeugen:
            triples = tripleGenerator.generate(body_string)
            print("Generated %s triples", triples.length)

            # HTTP Header:
            self.send_response(200)
            self.send_header("Content-type", "application/json")
            self.end_headers()
            
            # Tripel als JSON in den Body schreiben:
            triples_serialized = json.dumps(triples)
            result_bytes = bytes(triples_serialized, "utf-8")
            self.wfile.write(result_bytes)
            print("Wrote %s bytes as response", result_bytes.length)
        else:
            print("unknown URI: %s", self.path)
            print("expected URI: /get-triples")
            self.send_response(500)
            self.end_headers()

if __name__ == "__main__":        
    webServer = HTTPServer((hostName, serverPort), MyRequestHandler)
    print("Server started http://%s:%s" % (hostName, serverPort))

    try:
        webServer.serve_forever()
    except KeyboardInterrupt:
        pass

    webServer.server_close()
    print("Server stopped.")