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
            Cv2.Transpose(srcImage, srcImage);
            Cv2.Flip(srcImage, srcImage, FlipMode.X);
            Cv2.Resize(srcImage, srcImage, new OpenCvSharp.Size(480, 640));
            using var grayImage = new Mat();
            using var detectedFaceGrayImage = new Mat();

            // Convert the image to grayscale
            Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);

            // Detect faces in the image
            var faces = _faceCascade.DetectMultiScale(
                image: grayImage,
                scaleFactor: 1.1,
                minNeighbors: 9,
                minSize: new OpenCvSharp.Size(60, 60)
            );

            Debug.WriteLine($"[FaceDetection] Found {faces} faces");
#if true
            // Define color for drawing rectangles and circles
            var faceColor = Scalar.FromRgb(0, 255, 0);

            // Process each detected face
            foreach (var faceRect in faces)
            {
                // Draw rectangle around detected face
                Cv2.Rectangle(srcImage, faceRect, faceColor, 3);

                // Crop the face region and convert it to grayscale
                using var detectedFaceImage = new Mat(srcImage, faceRect);
                Cv2.CvtColor(detectedFaceImage, detectedFaceGrayImage, ColorConversionCodes.BGR2GRAY);

                // Detect eyes in the face region
                var nestedObjects = _eyeCascade.DetectMultiScale(
                    image: detectedFaceGrayImage,
                    minSize: new OpenCvSharp.Size(30, 30)
                );

                // Draw circles around detected eyes
                foreach (var nestedObject in nestedObjects)
                {
                    var center = new OpenCvSharp.Point
                    {
                        X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                        Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                    };
                    var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                    Cv2.Circle(srcImage, center, (int)radius, faceColor, thickness: 2);
                }
            }
#endif
            // Return the processed image
            return (faces, srcImage);
        }
    }
}
