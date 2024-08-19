using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperSharp
{
    public class WhisperUtil
    {
        public const int WHISPER_SAMPLE_RATE = 16000;
        public const int WHISPER_N_FFT = 400;
        public const int WHISPER_N_MEL = 80;
        public const int WHISPER_HOP_LENGTH = 160;
        public const int WHISPER_CHUNK_SIZE = 30;
        public const int WHISPER_MEL_LEN = 3000;

        private readonly WhisperVocab vocab = new WhisperVocab();
        private readonly WhisperFilter filters = new WhisperFilter();
        private readonly WhisperMel mel = new WhisperMel();

        public int GetTokenTranslate() => vocab.tokenTRANSLATE;
        public int GetTokenTranscribe() => vocab.tokenTRANSCRIBE;
        public int GetTokenEOT() => vocab.tokenEOT;
        public int GetTokenSOT() => vocab.tokenSOT;
        public int GetTokenPREV() => vocab.tokenPREV;
        public int GetTokenSOLM() => vocab.tokenSOLM;
        public int GetTokenNOT() => vocab.tokenNOT;
        public int GetTokenBEG() => vocab.tokenBEG;

        public string GetWordFromToken(int token) => vocab.tokenToWord.ContainsKey(token) ? vocab.tokenToWord[token] : string.Empty;

        // Load filters and vocab data from pre-generated filters_vocab_en.bin file
        public bool LoadFiltersAndVocab(bool multilingual, string vocabPath)
        {
            byte[] bytes = File.ReadAllBytes(vocabPath);
            using var vocabBuf = new MemoryStream(bytes);
            using var reader = new BinaryReader(vocabBuf);

            // Magic number check
            int magic = reader.ReadInt32();
            if (magic != 0x5553454E)
            {
                Console.WriteLine($"Invalid vocab file (bad magic: {magic}), {vocabPath}");
                return false;
            }

            // Load mel filters
            filters.nMel = reader.ReadInt32();
            filters.nFft = reader.ReadInt32();
            filters.data = new float[filters.nMel * filters.nFft];
            for (int i = 0; i < filters.data.Length; i++)
            {
                filters.data[i] = reader.ReadSingle();
            }

            // Load vocabulary
            int nVocab = reader.ReadInt32();
            for (int i = 0; i < nVocab; i++)
            {
                int len = reader.ReadInt32();
                byte[] wordBytes = reader.ReadBytes(len);
                string word = System.Text.Encoding.UTF8.GetString(wordBytes);
                vocab.tokenToWord[i] = word;
            }

            // Add additional vocab ids
            int nVocabAdditional = multilingual ? vocab.nVocabMultilingual : vocab.nVocabEnglish;

            if (multilingual)
            {
                vocab.tokenEOT++;
                vocab.tokenSOT++;
                vocab.tokenPREV++;
                vocab.tokenSOLM++;
                vocab.tokenNOT++;
                vocab.tokenBEG++;
            }

            for (int i = nVocab; i < nVocabAdditional; i++)
            {
                string word = i switch
                {
                    _ when i > vocab.tokenBEG => $"[_TT_{i - vocab.tokenBEG}]",
                    _ when i == vocab.tokenEOT => "[_EOT_]",
                    _ when i == vocab.tokenSOT => "[_SOT_]",
                    _ when i == vocab.tokenPREV => "[_PREV_]",
                    _ when i == vocab.tokenNOT => "[_NOT_]",
                    _ when i == vocab.tokenBEG => "[_BEG_]",
                    _ => $"[_extra_token_{i}]"
                };

                vocab.tokenToWord[i] = word;
            }

            return true;
        }

        // nSamples size => WHISPER_SAMPLE_RATE * WHISPER_CHUNK_SIZE => 480000
        public float[] GetMelSpectrogram(float[] samples, int nSamples, int nThreads)
        {
            int fftSize = WHISPER_N_FFT;
            int fftStep = WHISPER_HOP_LENGTH;

            mel.nMel = WHISPER_N_MEL;
            mel.nLen = nSamples / fftStep;
            mel.data = new float[mel.nMel * mel.nLen];

            float[] hann = new float[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                hann[i] = (float)(0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / fftSize)));
            }

            int nFft = 1 + fftSize / 2;

            // Multithreaded mel calculation
            var workers = new List<System.Threading.Tasks.Task>();
            for (int iw = 0; iw < nThreads; iw++)
            {
                int ith = iw;
                workers.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    float[] fftIn = new float[fftSize];
                    Array.Fill(fftIn, 0.0f);
                    float[] fftOut = new float[fftSize * 2];

                    for (int i = ith; i < mel.nLen; i += nThreads)
                    {
                        int offset = i * fftStep;

                        // apply Hanning window
                        for (int j = 0; j < fftSize; j++)
                        {
                            fftIn[j] = offset + j < nSamples ? hann[j] * samples[offset + j] : 0.0f;
                        }

                        // FFT -> mag^2
                        FFT(fftIn, fftOut);
                        for (int j = 0; j < fftSize; j++)
                        {
                            fftOut[j] = fftOut[2 * j] * fftOut[2 * j] + fftOut[2 * j + 1] * fftOut[2 * j + 1];
                        }

                        for (int j = 1; j < fftSize / 2; j++)
                        {
                            fftOut[j] += fftOut[fftSize - j];
                        }

                        // mel spectrogram
                        for (int j = 0; j < mel.nMel; j++)
                        {
                            double sum = 0.0;
                            for (int k = 0; k < nFft; k++)
                            {
                                sum += (fftOut[k] * filters.data[j * nFft + k]);
                            }

                            sum = Math.Max(sum, 1e-10);
                            sum = Math.Log10(sum);
                            mel.data[j * mel.nLen + i] = (float)sum;
                        }
                    }
                }));
            }

            System.Threading.Tasks.Task.WaitAll(workers.ToArray());

            // clamping and normalization
            double mmax = mel.data.Max();

            mmax -= 8.0;
            for (int i = 0; i < mel.nMel * mel.nLen; i++)
            {
                mel.data[i] = (float)Math.Max(mel.data[i], mmax);
                mel.data[i] = (float)((mel.data[i] + 4.0) / 4.0);
            }

            return mel.data;
        }

        private void DFT(float[] input, float[] output)
        {
            int inSize = input.Length;
            for (int k = 0; k < inSize; k++)
            {
                float re = 0.0f;
                float im = 0.0f;
                for (int n = 0; n < inSize; n++)
                {
                    float angle = (float)(2 * Math.PI * k * n / inSize);
                    re += input[n] * (float)Math.Cos(angle);
                    im -= input[n] * (float)Math.Sin(angle);
                }
                output[k * 2] = re;
                output[k * 2 + 1] = im;
            }
        }

        private void FFT(float[] input, float[] output)
        {
            int inSize = input.Length;
            if (inSize == 1)
            {
                output[0] = input[0];
                output[1] = 0.0f;
                return;
            }

            if (inSize % 2 == 1)
            {
                DFT(input, output);
                return;
            }

            float[] even = new float[inSize / 2];
            float[] odd = new float[inSize / 2];

            for (int i = 0; i < inSize; i++)
            {
                if (i % 2 == 0)
                {
                    even[i / 2] = input[i];
                }
                else
                {
                    odd[i / 2] = input[i];
                }
            }

            float[] evenFft = new float[inSize];
            float[] oddFft = new float[inSize];

            FFT(even, evenFft);
            FFT(odd, oddFft);

            for (int k = 0; k < inSize / 2; k++)
            {
                float theta = (float)(2 * Math.PI * k / inSize);
                float re = (float)Math.Cos(theta);
                float im = (float)-Math.Sin(theta);
                float reOdd = oddFft[2 * k];
                float imOdd = oddFft[2 * k + 1];
                output[2 * k] = evenFft[2 * k] + re * reOdd - im * imOdd;
                output[2 * k + 1] = evenFft[2 * k + 1] + re * imOdd + im * reOdd;
                output[2 * (k + inSize / 2)] = evenFft[2 * k] - re * reOdd + im * imOdd;
                output[2 * (k + inSize / 2) + 1] = evenFft[2 * k + 1] - re * imOdd - im * reOdd;
            }
        }

        private class WhisperVocab
        {
            public int tokenEOT = 50256;
            public int tokenSOT = 50257;
            public int tokenPREV = 50360;
            public int tokenSOLM = 50361;
            public int tokenNOT = 50362;
            public int tokenBEG = 50363;

            public readonly int tokenTRANSLATE = 50358;
            public readonly int tokenTRANSCRIBE = 50359;

            public readonly int nVocabEnglish = 51864;
            public readonly int nVocabMultilingual = 51865;

            public readonly Dictionary<int, string> tokenToWord = new Dictionary<int, string>();
        }

        private class WhisperFilter
        {
            public int nMel;
            public int nFft;
            public float[] data;
        }

        private class WhisperMel
        {
            public int nLen;
            public int nMel;
            public float[] data;
        }

        private class InputLang
        {
            public string Name { get; }
            public string Code { get; }
            public long Id { get; }

            public InputLang(string name, string code, long id)
            {
                Name = name;
                Code = code;
                Id = id;
            }

            public static List<InputLang> GetLangList()
            {
                return new List<InputLang>
                {
                    new InputLang("English", "en", 50259),
                    new InputLang("Spanish", "es", 50262),
                    new InputLang("Hindi", "hi", 50276),
                    new InputLang("Telugu", "te", 50299)
                };
            }
        }
    }
}
