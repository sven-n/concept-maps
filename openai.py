import openai

openai.ChatCompletion.create(
  model="gpt-3.5-turbo",
  messages=[
        {"role": "system", "content": "You are a helpful assistant."},
        {"role": "user", "content": "Who won the world series in 2020?"},
        {"role": "assistant", "content": "The Los Angeles Dodgers won the World Series in 2020."},
        {"role": "user", "content": "Where was it played?"}
    ]
)

# Setze deine OpenAI API-Zugriffsschlüssel
openai.api_key = '$OPENAI_API_KEY'

def generate_sentences():
    prompt = "Bob and Alice are brother and sister. Generate 20 sentences where this relationship is included."

    response = openai.Completion.create(
        engine="text-davinci-003",
        prompt=prompt,
        max_tokens=100,
        n=20,
        stop=None,
        temperature=0.7
    )

    sentences = [choice['text'].strip() for choice in response.choices]
    return sentences

# Aufruf der Funktion
generated_sentences = generate_sentences()

# Speichern in eine .txt-Datei
with open('output.txt', 'w') as file:
    for sentence in generated_sentences:
        file.write(sentence + '\n')
