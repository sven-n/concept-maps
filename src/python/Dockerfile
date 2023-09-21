FROM python:3.11.4

RUN pip install -U pip setuptools wheel
RUN pip install -U 'spacy[transformers,lookups]==3.5.4'
RUN python -m spacy download en_core_web_trf

WORKDIR /app
COPY python/ .
EXPOSE 5001

CMD ["python", "service.py"]