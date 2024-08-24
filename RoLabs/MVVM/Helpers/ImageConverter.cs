#if ANDROID
using Android.Graphics;
using Java.Nio;
#endif
using Microsoft.Maui.Graphics.Platform;
using OpenCvSharp;
using SkiaSharp;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rolabs.MVVM.Helpers
{
    public class ImageConverter
    {
        public static byte[]? ResizeImageByteArray(byte[] originalImageData, int width, int height)
        {
            byte[] pixelData = null;
#if ANDROID
            // Decode the byte array into a Bitmap
            Bitmap originalBitmap = BitmapFactory.DecodeByteArray(originalImageData, 0, originalImageData.Length);

            // Resize the Bitmap to the specified width and height
            Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(originalBitmap, width, height, true);

            // Calculate the size of the byte array
            int bytesPerPixel = resizedBitmap.HasAlpha ? 4 : 3; // ARGB_8888 or RGB_888
            int stride = width * bytesPerPixel;
            pixelData = new byte[stride * height];

            // Copy the pixel data into the byte array
            using (var buffer = ByteBuffer.Wrap(pixelData))
            {
                resizedBitmap.CopyPixelsToBuffer(buffer);
            }
#endif
            // Return the raw pixel data
            return pixelData;
        }

        public static IImage LoadImageFromByteArray(byte[] imageData)
        {
            using (var memoryStream = new MemoryStream(imageData))
            {
                return PlatformImage.FromStream(memoryStream);
            }
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
