using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Util; // Assuming you're using Android for logging
using Rolabs.MVVM.Helpers;
using WhisperSharp;

namespace Rolabs.MVVM.Services
{
    public class SpeechRecognition
    {
        private string _modelPath;
        private string _vocabPath;
        private Whisper _mWhisper;
        private string _testedAudio;

        public delegate void SpeechRecognitionResultCallback(string result);

        private SpeechRecognitionResultCallback _resultAvailableCallback { set; get; }

        public SpeechRecognition(SpeechRecognitionResultCallback resultCallback=null)
        {
            _resultAvailableCallback = resultCallback;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            // English-only model and vocab paths
            _modelPath = await Utils.CopyFileToAppDataDirectory2("aimodels\\whisper-tiny-en.tflite");
            _vocabPath = await Utils.CopyFileToAppDataDirectory2("filters_vocab_en.bin");
            _testedAudio = await Utils.CopyFileToAppDataDirectory2("audiotemp.wav");

            Debug.WriteLine("Speech recognition coppied files completed");

            _mWhisper = new Whisper();
            _mWhisper.LoadModel(_modelPath, _vocabPath, isMultilingual: false);

            var listener = new WhisperListener();
            listener.Callback = _resultAvailableCallback;

            _mWhisper.SetListener(listener);
            StartTranscription(_testedAudio);
        }

        // Transcription calls
        public void StartTranscription(string waveFilePath)
        {
            _mWhisper.SetFilePath(waveFilePath);
            _mWhisper.SetAction(Whisper.ActionTranscribe);
            _mWhisper.Start();
        }

        public void StopTranscription()
        {
            _mWhisper.Stop();
        }

        private class WhisperListener : IWhisperListener
        {
            private static readonly string TAG = nameof(WhisperListener);

            public SpeechRecognitionResultCallback Callback { set; get; }

            public void OnUpdateReceived(string message)
            {
                Debug.WriteLine(TAG, "Update received, Message: " + message);

                // Update your UI with the new status
                // Assuming you're on a platform where you have a way to update the UI
                // For example, using a handler in Android or a dispatcher in WPF, etc.

                if (message.Equals(Whisper.MsgProcessing))
                {
                    // Clear the result view (pseudo code, adjust to your environment)
                    Debug.WriteLine(TAG, "Processing ...");
                }
                else if (message.Equals(Whisper.MsgFileNotFound))
                {
                    Debug.WriteLine(TAG, "File not found error...!");
                    // Handle the file not found error as needed
                }
            }

            public void OnResultReceived(string result)
            {
                if(Callback != null)
                {
                    Callback(result);
                }
                Debug.WriteLine(TAG, "Result: " + result);
            }
        }
    }
}
