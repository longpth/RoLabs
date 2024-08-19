using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace WhisperSharp
{
    public class TFLiteEngineWrapper : IDisposable
    {
        private const string DllExtern = "libWhisper_Android.so";

        // Import the CreateTFLiteEngine function from the shared library
        [Pure, DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr createTFLiteEngine();

        // Import the LoadModel function from the shared library
        [Pure, DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern int loadModel(IntPtr nativePtr, string modelPath, bool isMultilingual);

        // Import the FreeModel function from the shared library
        [Pure, DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern void freeModel(IntPtr nativePtr);

        // Import the TranscribeBuffer function from the shared library
        [Pure, DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr transcribeBuffer(IntPtr nativePtr, float[] samples, int length);

        // Import the TranscribeFile function from the shared library
        [Pure, DllImport(DllExtern, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        private static extern IntPtr transcribeFile(IntPtr nativePtr, string waveFile);

        private IntPtr _enginePtr;

        // Singleton instance
        private static readonly Lazy<TFLiteEngineWrapper> _instance = new Lazy<TFLiteEngineWrapper>(() => new TFLiteEngineWrapper());

        // Private constructor to prevent direct instantiation
        private TFLiteEngineWrapper()
        {
            _enginePtr = createTFLiteEngine();
        }

        // Public static property to access the singleton instance
        public static TFLiteEngineWrapper Instance => _instance.Value;

        // LoadModel method
        public int LoadModel(string modelPath, bool isMultilingual=false)
        {
            if (_enginePtr == IntPtr.Zero)
                throw new InvalidOperationException("Engine not initialized.");

            var ret = -1;

            try
            {
                ret = loadModel(_enginePtr, modelPath, isMultilingual);
            }

            catch (Exception ex) {
                Debug.WriteLine(ex);
            }

            return ret;
        }

        // TranscribeBuffer method
        public IntPtr TranscribeBuffer(float[] samples)
        {
            if (_enginePtr == IntPtr.Zero)
                throw new InvalidOperationException("Engine not initialized.");

            IntPtr resultPtr = transcribeBuffer(_enginePtr, samples, samples.Length);
            return resultPtr;
        }

        // TranscribeFile method
        public string TranscribeFile(string waveFile)
        {
            if (_enginePtr == IntPtr.Zero)
                throw new InvalidOperationException("Engine not initialized.");

            IntPtr resultPtr = transcribeFile(_enginePtr, waveFile);
            return Marshal.PtrToStringUTF8(resultPtr);
        }

        // Dispose method for cleaning up resources
        public void Dispose()
        {
            if (_enginePtr != IntPtr.Zero)
            {
                freeModel(_enginePtr);
                _enginePtr = IntPtr.Zero;
            }
        }

        ~TFLiteEngineWrapper()
        {
            Dispose();
        }
    }
}
