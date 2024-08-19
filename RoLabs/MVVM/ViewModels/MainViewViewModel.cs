using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Timers;
using Rolabs.MVVM.Services;
using Plugin.Maui.Audio;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Rolabs.MVVM.Helpers;

namespace Rolabs.MVVM.ViewModels
{
    public class MainViewViewModel : BaseViewModel
    {
        private bool _isMicOn;
        private static Timer _aTimer;
        private SpeechRecognition _speechRecognition;
        private string _wavPath;
        private string _speechRecognitionResult;

        // Singleton instance
        private static MainViewViewModel instance = null;
        public static MainViewViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainViewViewModel();
                }
                return instance;
            }
        }

        // Private constructor to prevent instantiation
        private MainViewViewModel()
        {
            //MicToggleCommand = new Command(ToggleMic);

            _speechRecognition = new SpeechRecognition(SpeechRecognitionAvailable);
        }

        public IAudioManager AudioManager { get; set; }
        public IAudioRecorder AudioRecorder { get; set; }

        public bool IsMicOn
        {
            get => _isMicOn;
            set
            {
                if (_isMicOn != value)
                {
                    _isMicOn = value;
                    ToggleMic();
                    OnPropertyChanged();
                }
            }
        }

        private async void StartRecording()
        {
            if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
            {
                // TODO Inform your user
                return;
            }

            if (!AudioRecorder.IsRecording)
            {
                AudioRecordingOptions options = new AudioRecordingOptions();
                options.SampleRate = 16000;
                await AudioRecorder.StartAsync(options);
            }
        }

        private async void StopRecording()
        {
            if (AudioRecorder.IsRecording)
            {
                var recordedAudio = await AudioRecorder.StopAsync();
                if (recordedAudio != null)
                {
                    var stream = recordedAudio.GetAudioStream();
                    _wavPath = await Utils.SaveFile(stream, "voice.wav");
                }
            }
        }

        private async void ToggleMic()
        {
            if (_isMicOn)
            {
                StartRecording();
                SetTimer();
            }
            else
            {
                StopTimer();
            }
        }

        public string SpeechRecognitionResult
        {
            get => _speechRecognitionResult;
            set
            {
                if (_speechRecognitionResult != value)
                {
                    _speechRecognitionResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private void SetTimer()
        {
            // Create a timer with a 15-second interval.
            _aTimer = new Timer(15000);
            // Hook up the Elapsed event for the timer.
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = false; // Ensure the timer runs only once
            _aTimer.Enabled = true;

            Console.WriteLine("The application started the timer at {0:HH:mm:ss.fff}", DateTime.Now);
        }

        private void StopTimer()
        {
            if (_aTimer != null)
            {
                _aTimer.Stop();
                _aTimer.Dispose();
                _aTimer = null;

                Console.WriteLine("The timer was stopped at {0:HH:mm:ss.fff}", DateTime.Now);
            }

            StopRecording();

            _speechRecognition.StartTranscription(_wavPath);
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // Automatically toggle off the mic
            IsMicOn = false;
            StopTimer();

            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        }

        private void SpeechRecognitionAvailable(string result)
        {
            SpeechRecognitionResult = result;
        }
    }
}
