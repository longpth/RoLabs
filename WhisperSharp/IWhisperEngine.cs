using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperSharp
{
    public interface IWhisperEngine : IDisposable
    {
        public bool IsInitialized { get; }
        public void Interrupt();
        public void SetUpdateListener(IWhisperListener listener);
        public bool Initialize(string modelPath, string vocabPath, bool multilingual);
        public string TranscribeFile(string wavePath);
        public string TranscribeBuffer(float[] samples);

        // Optional method for translation, uncomment if needed
        // string GetTranslation(string wavePath);
    }
}