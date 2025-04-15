let mediaRecorder;
let audioChunks = [];
let isRecording = false;

function showTab(tabId) {
  document
    .querySelectorAll(".tab-content")
    .forEach((tab) => tab.classList.remove("active"));
  document
    .querySelectorAll(".tab-button")
    .forEach((btn) => btn.classList.remove("active"));
  document.getElementById(tabId).classList.add("active");
  document
    .querySelector(`button[onclick="showTab('${tabId}')"]`)
    .classList.add("active");
}

async function convertToSpeech() {
  const inputText = document.getElementById("inputText").value;
  const statusDiv = document.getElementById("ttsStatus");
  const audioPlayer = document.getElementById("audioPlayer");
  const button = document.querySelector("#tts button");

  if (!inputText) {
    statusDiv.className = "status error";
    statusDiv.textContent = "Please enter some text.";
    return;
  }

  try {
    button.disabled = true;
    statusDiv.className = "status";
    statusDiv.textContent = "Generating audio...";

    const response = await fetch("/text-to-speech", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ Text: inputText }),
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const audioBlob = await response.blob();
    const audioUrl = URL.createObjectURL(audioBlob);

    audioPlayer.src = audioUrl;
    audioPlayer.style.display = "block";
    await audioPlayer.play();

    statusDiv.className = "status success";
    statusDiv.textContent = "Audio generated successfully!";
  } catch (error) {
    statusDiv.className = "status error";
    statusDiv.textContent = `Error occurred: ${error.message}`;
    console.error("Error:", error);
  } finally {
    button.disabled = false;
  }
}

async function toggleRecording() {
  const recordButton = document.getElementById("recordButton");
  const statusDiv = document.getElementById("sttStatus");

  if (!isRecording) {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      mediaRecorder = new MediaRecorder(stream);
      audioChunks = [];

      mediaRecorder.ondataavailable = (event) => {
        audioChunks.push(event.data);
      };

      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(audioChunks, { type: "audio/wav" });
        await convertSpeechToText(audioBlob);
      };

      mediaRecorder.start();
      isRecording = true;
      recordButton.textContent = "Stop Recording";
      recordButton.classList.add("recording");
      statusDiv.className = "status success";
      statusDiv.textContent = "Recording in progress...";
    } catch (error) {
      statusDiv.className = "status error";
      statusDiv.textContent = `Microphone access error: ${error.message}`;
    }
  } else {
    mediaRecorder.stop();
    isRecording = false;
    recordButton.textContent = "Start Recording";
    recordButton.classList.remove("recording");
  }
}

document
  .getElementById("audioFile")
  .addEventListener("change", async (event) => {
    const file = event.target.files[0];
    if (file) {
      await convertSpeechToText(file);
    }
  });

async function convertSpeechToText(audioData) {
  const statusDiv = document.getElementById("sttStatus");
  const outputText = document.getElementById("outputText");

  try {
    statusDiv.className = "status";
    statusDiv.textContent = "Converting speech to text...";

    const formData = new FormData();
    formData.append("audio", audioData);

    const response = await fetch("/speech-to-text", {
      method: "POST",
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    outputText.value = result.text;
    statusDiv.className = "status success";
    statusDiv.textContent = "Speech successfully converted to text!";
  } catch (error) {
    statusDiv.className = "status error";
    statusDiv.textContent = `Error occurred: ${error.message}`;
    console.error("Error:", error);
  }
}
