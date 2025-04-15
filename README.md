# Text to Speech & Speech to Text

A modern, responsive web application for converting text to speech and speech to text using Hugging Face's advanced AI models.
*There may be errors due to the models, but this is not a problem, if you keep trying after the error messages you will get results.

![metinden](https://github.com/user-attachments/assets/4582201a-8784-40ec-83cf-7284c77ddc00)

![konusmadan](https://github.com/user-attachments/assets/fec910fb-d7b4-47fa-9e31-09c674924b5e)

## Features

- **Text-to-Speech Conversion**: Convert written text to natural-sounding speech
- **Speech-to-Text Conversion**: Transcribe spoken words from audio recordings or uploaded files
- **Modern UI**: Clean, responsive design with smooth animations and transitions
- **Real-time Processing**: Quick conversion with instant playback and display
- **File Upload Support**: Upload existing audio files for transcription
- **Recording Capability**: Record your voice directly through the browser

## Technology Stack

- **Frontend**: HTML5, CSS3, JavaScript
- **Backend**: ASP.NET Core 9.0
- **APIs**: Hugging Face Inference API
  - Text-to-Speech: facebook/mms-tts-tur
  - Speech-to-Text: openai/whisper-large-v3-turbo

## Prerequisites

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) or newer
- [Hugging Face API Key](https://huggingface.co/settings/tokens)

## Setup and Installation

1. **Clone the repository**

```bash
git clone https://github.com/yildizahmethakan0/TTS--STT.git
cd AI_TextToSpeech_And_SpeechToText
```

2. **Configure API Key**

Create an `appsettings.json` file in the project root or modify the existing one:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "HuggingFace": {
    "APIKey": "YOUR_HUGGINGFACE_API_KEY_HERE"
  }
}
```

3. **Build and run the application**

```bash
dotnet build
dotnet run
```

4. **Access the application**

Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## Usage

### Text-to-Speech

1. Navigate to the "Text-to-Speech" tab
2. Enter the desired text in the input field
3. Click "Convert to Speech" button
4. The audio will be generated and automatically played back
5. Audio files are saved in the `/wwwroot/audio_outputs` directory

### Speech-to-Text

1. Navigate to the "Speech-to-Text" tab
2. Either:
   - Click "Start Recording" to record your voice through the microphone
   - Upload an existing audio file by clicking "Select Audio File"
3. The transcribed text will appear in the output field

## Project Structure

```
voice-converter/
├── Program.cs                # Main application entry point and API endpoints
├── TextToSpeechRequest.cs    # Text-to-speech request model
├── wwwroot/                  # Static files
│   ├── index.html            # Main HTML file
│   ├── style.css             # CSS styles
│   ├── script.js             # JavaScript functionality
│   └── audio_outputs/        # Generated audio files (created at runtime)
├── appsettings.json          # Application settings including API keys
└── README.md                 # Project documentation
```

## API Endpoints

### POST /text-to-speech

Converts text to speech using the facebook/mms-tts-tur model.

**Request Body**:
```json
{
  "Text": "Your text to convert to speech"
}
```

**Response**: Audio file (WAV format)

### POST /speech-to-text

Converts speech to text using the openai/whisper-large-v3-turbo model.

**Request Body**: Form data with an audio file (key: "audio")

**Response**:
```json
{
  "Text": "Transcribed text from the audio"
}
```

## Customization

### Changing Language Models

You can modify the Hugging Face models used in `Program.cs`:

- For text-to-speech, change the model in the line:
  ```csharp
  HttpResponseMessage response = await httpClient.PostAsync("models/facebook/mms-tts-tur", stringContent);
  ```

- For speech-to-text, change the model in the line:
  ```csharp
  HttpResponseMessage response = await httpClient.PostAsync("models/openai/whisper-large-v3-turbo", byteArrayContent);
  ```


