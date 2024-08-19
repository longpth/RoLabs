using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WhisperSharp
{
    public class WhisperEngineNative : IWhisperEngine
    {
        private readonly string TAG = "WhisperEngineNative";
        private bool _isInitialized = false;
        private IWhisperListener mUpdateListener = null;

        public void SetUpdateListener(IWhisperListener listener)
        {
            mUpdateListener = listener;
        }

        public bool IsInitialized { get { return _isInitialized; } }

        public bool Initialize(string modelPath, string vocabPath, bool multilingual)
        {
            int ret = LoadModel(modelPath);
            Console.WriteLine(TAG, "Model is loaded..." + modelPath);

            if (ret == 0)
            {
                _isInitialized = true;
            }
            return _isInitialized;
        }


        public string TranscribeBuffer(float[] samples)
        {
            IntPtr resultPtr = TFLiteEngineWrapper.Instance.TranscribeBuffer(samples);
            return Marshal.PtrToStringUTF8(resultPtr);
        }


        public string TranscribeFile(string waveFile)
        {
            return TFLiteEngineWrapper.Instance.TranscribeFile(waveFile);
        }


        public void Interrupt()
        {

        }

        private int LoadModel(string modelPath)
        {
            int loadResult = TFLiteEngineWrapper.Instance.LoadModel(modelPath);

            return loadResult;
        }

        public void Dispose()
        {
            TFLiteEngineWrapper.Instance.Dispose();
        }
    }
}
