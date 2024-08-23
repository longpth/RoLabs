using Microsoft.Maui.Controls;
using OpenCvSharp;
using Rolabs.MVVM.Helpers;
using Rolabs.MVVM.ViewModels;
using System;
using System.Diagnostics;

namespace RoLabs.MVVM.Services
{
    public class FaceDetection
    {
        private readonly CascadeClassifier _faceCascade;
        private readonly CascadeClassifier _eyeCascade;
        private string haarcascade_frontalface_alt_path;
        private string haarcascade_eye_path;

        private static FaceDetection instance = null;
        public static FaceDetection Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FaceDetection();
                }
                return instance;
            }
        }

        private FaceDetection()
        {
            haarcascade_frontalface_alt_path = Utils.CopyFileToAppDataDirectory("haarcascades\\haarcascade_frontalface_default.xml").GetAwaiter().GetResult();
            haarcascade_eye_path = Utils.CopyFileToAppDataDirectory("haarcascades\\haarcascade_eye.xml").GetAwaiter().GetResult();
            // Load the cascade classifiers for face and eye detection
            _faceCascade = new CascadeClassifier(haarcascade_frontalface_alt_path);
            _eyeCascade = new CascadeClassifier(haarcascade_eye_path);
        }

        public (OpenCvSharp.Rect[], Mat) DetectFaces(byte[] imageData)
        {
            // Convert byte[] to Mat
            Mat srcImage = Mat.FromImageData(imageData, ImreadModes.Color);

            // Transpose the image (rotate 90 degrees)
            Cv2.Transpose(srcImage, srcImage);

            // Flip the image horizontally if it's from the front camera
            if (CameraViewModel.Instance.Camera.Name.Contains("Front"))
            {
                Cv2.Flip(srcImage, srcImage, FlipMode.X);
            }

#if false
            string cacheFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsoluteFile.Path.ToString();
            cacheFolder = cacheFolder + System.IO.Path.DirectorySeparatorChar;
            var fileNameToSave = cacheFolder + "faceDetection.jpg";
            Cv2.ImWrite(fileNameToSave, srcImage);
#endif

            // Get the original image size
            var originalSize = srcImage.Size();

            // Resize the image to the processing size
            var processingSize = new OpenCvSharp.Size(Global.VisionWidth, Global.VisionHeight);
            Mat srcImageResized = new Mat();
            Cv2.Resize(srcImage, srcImageResized, processingSize);

            // Calculate the scale factors for resizing back to the original size
            double scaleX = (double)originalSize.Width / processingSize.Width;
            double scaleY = (double)originalSize.Height / processingSize.Height;

            using var grayImage = new Mat();
            using var detectedFaceGrayImage = new Mat();

            // Convert the resized image to grayscale for processing
            Cv2.CvtColor(srcImageResized, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);

            // Detect faces in the resized image
            var faces = _faceCascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 9,
                minSize: new OpenCvSharp.Size(60, 60)
            );

            Debug.WriteLine($"[FaceDetection] Found {faces.Length} faces");

#if false
            // Define color for drawing rectangles and circles
            var faceColor = Scalar.FromRgb(0, 255, 0);

            // Process each detected face in the resized image
            foreach (var faceRect in faces)
            {
                // Scale the face rectangle to the original image size
                var scaledFaceRect = new OpenCvSharp.Rect(
                    (int)(faceRect.X * scaleX),
                    (int)(faceRect.Y * scaleY),
                    (int)(faceRect.Width * scaleX),
                    (int)(faceRect.Height * scaleY)
                );

                // Draw rectangle around detected face on the original image
                Cv2.Rectangle(srcImage, scaledFaceRect, faceColor, 3);

                // Crop the face region from the resized image for further processing
                using var detectedFaceImage = new Mat(srcImageResized, faceRect);
                Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGR2GRAY);

                // Detect eyes in the face region
                var nestedObjects = _eyeCascade.DetectMultiScale(
                    image: detectedFaceGrayImage,
                    minSize: new OpenCvSharp.Size(30, 30)
                );

                // Draw circles around detected eyes on the original image
                foreach (var nestedObject in nestedObjects)
                {
                    var center = new OpenCvSharp.Point
                    {
                        X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) * scaleX + scaledFaceRect.Left),
                        Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) * scaleY + scaledFaceRect.Top)
                    };
                    var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25 * scaleX, MidpointRounding.ToEven);
                    Cv2.Circle(srcImage, center, (int)radius, faceColor, thickness: 2);
                }
            }
#endif
            // Return the scaled face rectangles and the original image with drawn annotations
            return (faces, srcImage);
        }
    }
}
