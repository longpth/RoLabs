using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperSharp
{
    public class WaveUtil
    {
        public const string TAG = "WaveUtil";
        public const string RECORDING_FILE = "MicInput.wav";

        public static void CreateWaveFile(string filePath, byte[] samples, int sampleRate, int numChannels, int bytesPerSample)
        {
            try
            {
                int dataSize = samples.Length; // actual data size in bytes
                int audioFormat = bytesPerSample switch
                {
                    2 => 1, // PCM_16
                    4 => 3, // PCM_FLOAT
                    _ => 0
                };

                using var fileOutputStream = new FileStream(filePath, FileMode.Create);
                using var writer = new BinaryWriter(fileOutputStream, Encoding.UTF8);

                // Write the "RIFF" chunk descriptor
                writer.Write(Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(IntToByteArray(36 + dataSize)); // Total file size - 8 bytes
                writer.Write(Encoding.UTF8.GetBytes("WAVE")); // Write the "WAVE" format
                writer.Write(Encoding.UTF8.GetBytes("fmt ")); // Write the "fmt " sub-chunk
                writer.Write(IntToByteArray(16)); // Sub-chunk size (16 for PCM)
                writer.Write(ShortToByteArray((short)audioFormat)); // Audio format (1 for PCM)
                writer.Write(ShortToByteArray((short)numChannels)); // Number of channels
                writer.Write(IntToByteArray(sampleRate)); // Sample rate
                writer.Write(IntToByteArray(sampleRate * numChannels * bytesPerSample)); // Byte rate
                writer.Write(ShortToByteArray((short)(numChannels * bytesPerSample))); // Block align
                writer.Write(ShortToByteArray((short)(bytesPerSample * 8))); // Bits per sample
                writer.Write(Encoding.UTF8.GetBytes("data")); // Write the "data" sub-chunk
                writer.Write(IntToByteArray(dataSize)); // Data size

                // Write audio samples
                writer.Write(samples);

                // Close the file output stream
            }
            catch (IOException e)
            {
                Console.WriteLine($"{TAG} Error: {e}");
            }
        }

        public static float[] GetSamples(string filePath)
        {
            try
            {
                using var fileInputStream = new FileStream(filePath, FileMode.Open);
                using var reader = new BinaryReader(fileInputStream);

                // Read the WAV file header
                byte[] header = reader.ReadBytes(44);

                // Check if it's a valid WAV file (contains "RIFF" and "WAVE" markers)
                string headerStr = Encoding.UTF8.GetString(header, 0, 4);
                if (!headerStr.Equals("RIFF"))
                {
                    Console.Error.WriteLine("Not a valid WAV file");
                    return Array.Empty<float>();
                }

                // Get the audio format details from the header
                int sampleRate = ByteArrayToNumber(header, 24, 4);
                int bitsPerSample = ByteArrayToNumber(header, 34, 2);
                if (bitsPerSample != 16 && bitsPerSample != 32)
                {
                    Console.Error.WriteLine($"Unsupported bits per sample: {bitsPerSample}");
                    return Array.Empty<float>();
                }

                // Get the size of the data section (all PCM data)
                int dataLength = (int)reader.BaseStream.Length - 44;

                // Calculate the number of samples
                int bytesPerSample = bitsPerSample / 8;
                int numSamples = dataLength / bytesPerSample;

                // Read the audio data
                byte[] audioData = reader.ReadBytes(dataLength);
                var byteBuffer = new MemoryStream(audioData);

                // Convert audio data to PCM_FLOAT format
                float[] samples = new float[numSamples];
                using (var bufferReader = new BinaryReader(byteBuffer))
                {
                    if (bitsPerSample == 16)
                    {
                        for (int i = 0; i < numSamples; i++)
                        {
                            samples[i] = bufferReader.ReadInt16() / 32768.0f;
                        }
                    }
                    else if (bitsPerSample == 32)
                    {
                        for (int i = 0; i < numSamples; i++)
                        {
                            samples[i] = bufferReader.ReadSingle();
                        }
                    }
                }

                return samples;
            }
            catch (IOException e)
            {
                Console.WriteLine($"{TAG} Error: {e}");
            }
            return Array.Empty<float>();
        }

        // Convert a portion of a byte array into an integer or a short
        private static int ByteArrayToNumber(byte[] bytes, int offset, int length)
        {
            int value = 0;

            for (int i = 0; i < length; i++)
            {
                value |= (bytes[offset + i] & 0xFF) << (8 * i);
            }

            return value;
        }

        private static byte[] IntToByteArray(int value)
        {
            return new byte[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF),
                (byte)((value >> 16) & 0xFF),
                (byte)((value >> 24) & 0xFF)
            };
        }

        private static byte[] ShortToByteArray(short value)
        {
            return new byte[]
            {
                (byte)(value & 0xFF),
                (byte)((value >> 8) & 0xFF)
            };
        }
    }
}
