using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WhisperSharp
{
    public class Whisper
    {
        public const string Tag = "Whisper";
        public const string ActionTranslate = "TRANSLATE";
        public const string ActionTranscribe = "TRANSCRIBE";
        public const string MsgProcessing = "Processing...";
        public const string MsgProcessingDone = "Processing done...!";
        public const string MsgFileNotFound = "Input file doesn't exist..!";

        private readonly object _audioBufferQueueLock = new object();
        private readonly object _whisperEngineLock = new object();
        private readonly Queue<float[]> _audioBufferQueue = new Queue<float[]>();
        private readonly IWhisperEngine _whisperEngine = new WhisperEngineNative();

        private Thread _micTranscribeThread = null;
        private string _action = null;
        private string _wavFilePath = null;
        private Thread _executorThread = null;
        private IWhisperListener _updateListener = null;
        private bool _inProgress = false;

        public Whisper()
        {
        }

        public void SetListener(IWhisperListener listener)
        {
            _updateListener = listener;
            _whisperEngine.SetUpdateListener(_updateListener);
        }

        public void LoadModel(string modelPath, string vocabPath, bool isMultilingual)
        {
            try
            {
                _whisperEngine.Initialize(modelPath, vocabPath, isMultilingual);

                // Start thread for mic data transcription in real-time
                //StartMicTranscriptionThread();
            }
            catch (IOException e)
            {
                Debug.WriteLine(Tag, "Error...", e);
            }
        }

        public void SetAction(string action)
        {
            _action = action;
        }

        public void SetFilePath(string wavFile)
        {
            _wavFilePath = wavFile;
        }

        public void Start()
        {
            if (_inProgress)
            {
                Debug.WriteLine(Tag, "Execution is already in progress...");
                return;
            }

            _executorThread = new Thread(() =>
            {
                _inProgress = true;
                ThreadFunction();
                _inProgress = false;
            });
            _executorThread.Start();
        }

        public void Stop()
        {
            _inProgress = false;
            try
            {
                if (_executorThread != null)
                {
                    _whisperEngine.Interrupt();
                    _executorThread.Join();
                    _executorThread = null;
                }
            }
            catch (ThreadInterruptedException e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public bool IsInProgress()
        {
            return _inProgress;
        }

        private void SendUpdate(string message)
        {
            _updateListener?.OnUpdateReceived(message);
        }

        private void SendResult(string message)
        {
            _updateListener?.OnResultReceived(message);
        }

        private void ThreadFunction()
        {
            try
            {
                if (_whisperEngine.IsInitialized)
                {
                    Debug.WriteLine(Tag, "WaveFile: " + _wavFilePath);

                    if (File.Exists(_wavFilePath))
                    {
                        long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        SendUpdate(MsgProcessing);

                        // Get result from wav file
                        lock (_whisperEngineLock)
                        {
                            string result = _whisperEngine.TranscribeFile(_wavFilePath);
                            SendResult(result);
                            Debug.WriteLine(Tag, "Result len: " + result.Length + ", Result: " + result);
                        }

                        SendUpdate(MsgProcessingDone);

                        long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        long timeTaken = endTime - startTime;
                        Debug.WriteLine(Tag, "Time Taken for transcription: " + timeTaken + "ms");
                    }
                    else
                    {
                        SendUpdate(MsgFileNotFound);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(Tag, "Error...", e);
                SendUpdate(e.Message);
            }
        }

        // Write buffer in Queue
        public void WriteBuffer(float[] samples)
        {
            lock (_audioBufferQueueLock)
            {
                _audioBufferQueue.Enqueue(samples);
                Monitor.Pulse(_audioBufferQueueLock); // Notify waiting threads
            }
        }

        // Read buffer from Queue
        private float[] ReadBuffer()
        {
            lock (_audioBufferQueueLock)
            {
                while (_audioBufferQueue.Count == 0)
                {
                    try
                    {
                        Monitor.Wait(_audioBufferQueueLock); // Wait for the queue to have data
                    }
                    catch (ThreadInterruptedException)
                    {
                        Thread.CurrentThread.Interrupt();
                    }
                }
                return _audioBufferQueue.Dequeue();
            }
        }

        // Mic data transcription thread in real-time
        private void StartMicTranscriptionThread()
        {
            if (_micTranscribeThread == null)
            {
                _micTranscribeThread = new Thread(() =>
                {
                    while (true)
                    {
                        float[] samples = ReadBuffer();
                        if (samples != null)
                        {
                            lock (_whisperEngineLock)
                            {
                                string result = _whisperEngine.TranscribeBuffer(samples);
                                SendResult(result);
                            }
                        }
                    }
                });

                _micTranscribeThread.Start();
            }
        }
    }
}
