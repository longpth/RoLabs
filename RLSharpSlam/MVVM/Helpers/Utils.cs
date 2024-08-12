using OpenCvSharp;
using SkiaSharp;

namespace RLSharpSlam.MVVM.Helpers
{
    public class Utils
    {
        public static async Task<string> CopyFileToAppDataDirectory(string filename)
        {
            // Open the source file
            using Stream inputStream = await FileSystem.Current.OpenAppPackageFileAsync(filename);

            // Create an output filename
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);

            // Copy the file to the AppDataDirectory
            using FileStream outputStream = File.Create(targetFile);
            await inputStream.CopyToAsync(outputStream);
            return targetFile;
        }
    }
    public static class MatExtensions
    {
        public static ImageSource ToImageSource(this Mat mat, string format = ".jpg")
        {
            // Convert Mat to byte array
            byte[] imageData = mat.ToBytes(format);

            // Create a MemoryStream from the byte array
            var imageStream = new MemoryStream(imageData);

            var imageSource = ImageSource.FromStream(() => imageStream);

            return imageSource;
        }
        public static SKBitmap ToSKBitmap(this Mat mat)
        {
            using (var stream = new MemoryStream())
            {
                mat.WriteToStream(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return SKBitmap.Decode(stream);
            }
        }

        // Convert a Stream to Mat
        public static Mat StreamToMat(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream), "Input stream cannot be null.");
            }

            try
            {
                // Read the stream into a byte array
                using (var memoryStream = new MemoryStream())
                {
                    inputStream.CopyTo(memoryStream);
                    byte[] imageData = memoryStream.ToArray();

                    // Load the byte array into a Mat
                    return Mat.FromImageData(imageData, ImreadModes.Color);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting stream to Mat: {ex.Message}");
                return new Mat(); // Return an empty Mat in case of an error
            }
        }


    }

    public class SKBitmapImageSource : StreamImageSource
    {
        public SKBitmap Bitmap { get; }

        public SKBitmapImageSource(SKBitmap bitmap)
        {
            Bitmap = bitmap;
            Stream = GetStreamAsync;
        }

        private Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
        {
            var stream = new MemoryStream();
            Bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult<Stream>(stream);
        }
    }

}
