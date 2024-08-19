using System;
using System.Runtime.InteropServices;

namespace WhisperSharp
{
    public class WhisperEngine : IWhisperEngine
    {
        private readonly WhisperUtil _whisperUtil = new WhisperUtil();
        private bool _isInitialized = false;
        private IWhisperListener _updateListener = null;

        private void LoadModel(string modelPath)
        {
            int loadResult = TFLiteEngineWrapper.Instance.LoadModel(modelPath);
            if (loadResult != 0)
            {
                throw new Exception("Failed to load Whisper model.");
            }
        }

        private float[] GetMelSpectrogram(string wavePath)
        {
            float[] samples = WaveUtil.GetSamples(wavePath);
            int fixedInputSize = WhisperUtil.WHISPER_SAMPLE_RATE * WhisperUtil.WHISPER_CHUNK_SIZE;
            float[] inputSamples = new float[fixedInputSize];
            int copyLength = Math.Min(samples.Length, fixedInputSize);
            Array.Copy(samples, 0, inputSamples, 0, copyLength);

            return _whisperUtil.GetMelSpectrogram(inputSamples, inputSamples.Length, Environment.ProcessorCount);
        }

        private string RunInference(float[] inputData)
        {
            IntPtr resultPtr = TFLiteEngineWrapper.Instance.TranscribeBuffer(inputData);
            return Marshal.PtrToStringUTF8(resultPtr);
        }

        public bool IsInitialized { get { return _isInitialized; } }

        public bool Initialize(string modelPath, string vocabPath, bool multilingual)
        {
            // Load TFLite model
            LoadModel(modelPath);

            // Load Vocab and Filters
            bool ret = _whisperUtil.LoadFiltersAndVocab(multilingual, vocabPath);
            if (ret)
            {
                _isInitialized = true;
                Console.WriteLine("Filters and Vocab loaded successfully.");
            }
            else
            {
                _isInitialized = false;
                Console.WriteLine("Failed to load Filters and Vocab.");
            }
            return _isInitialized;
        }

        public void Interrupt()
        {

        }

        public void SetUpdateListener(IWhisperListener listener)
        {
            _updateListener = listener;
        }

        public string TranscribeFile(string wavePath)
        {
            float[] melSpectrogram = GetMelSpectrogram(wavePath);
            return RunInference(melSpectrogram);
        }

        public string TranscribeBuffer(float[] samples)
        {
            return RunInference(samples);
        }

        public void Dispose()
        {
           TFLiteEngineWrapper.Instance.Dispose();
        }
    }
}
