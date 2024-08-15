using Android.Graphics;
using Java.Nio;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Rolabs.MVVM.Helpers
{
    public class ImageConverter
    {
        public static byte[] ResizeImageByteArray(byte[] originalImageData, int width, int height)
        {
            // Decode the byte array into a Bitmap
            Bitmap originalBitmap = BitmapFactory.DecodeByteArray(originalImageData, 0, originalImageData.Length);

            // Resize the Bitmap to the specified width and height
            Bitmap resizedBitmap = Bitmap.CreateScaledBitmap(originalBitmap, width, height, true);

            // Calculate the size of the byte array
            int bytesPerPixel = resizedBitmap.HasAlpha ? 4 : 3; // ARGB_8888 or RGB_888
            int stride = width * bytesPerPixel;
            byte[] pixelData = new byte[stride * height];

            // Copy the pixel data into the byte array
            using (var buffer = ByteBuffer.Wrap(pixelData))
            {
                resizedBitmap.CopyPixelsToBuffer(buffer);
            }

            // Return the raw pixel data
            return pixelData;
        }
    }

}
