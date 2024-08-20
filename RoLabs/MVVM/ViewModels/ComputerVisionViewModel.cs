using RoLabs.MVVM.Services;
using OpenCvSharp;
using Rolabs.MVVM.Helpers;
using Rolabs.MVVM.CustomViews;
using System.Collections.ObjectModel;

namespace Rolabs.MVVM.ViewModels
{
    public class ComputerVisionViewModel: BaseViewModel
    {

        private readonly int ProcessWidth = 480, ProcessHeight = 640;

        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged();
                }
            }
        }

        private IDrawable _cameraViewCanvas;
        public IDrawable CameraViewCanvas
        {
            get => _cameraViewCanvas;
            set
            {
                if (_cameraViewCanvas != value)
                {
                    _cameraViewCanvas = value;
                    OnPropertyChanged(nameof(CameraViewCanvas));
                }
            }
        }

        private ComputerVisionViewModel()
        {
            // Initialize the Detectors collection with available options
            Detectors = new ObservableCollection<string>
            {
                "Slam",
                "FaceDetection"
            };

            // Set a default selected detector if needed
            Detector = Detectors.FirstOrDefault();
        }

        private static ComputerVisionViewModel _instance = null;
        public static ComputerVisionViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComputerVisionViewModel();
                }
                return _instance;
            }
        }

        // Property for available detectors
        private string _detector;
        public ObservableCollection<string> Detectors { get; }

        // Property for the selected detector
        public string Detector
        {
            get => _detector;
            set
            {
                if (_detector != value)
                {
                    _detector = value;
                    OnPropertyChanged(nameof(Detector));
                }
            }
        }

        private void ProcessFaceDetection(byte[] imageData)
        {
            // Perform face detection
            var (detectedFaces, tmp) = FaceDetection.Instance.DetectFaces(imageData);

            // Convert OpenCvSharp.Rect to Microsoft.Maui.Graphics.Rect for drawing
            var faceRectangles = detectedFaces.Select(face => new Microsoft.Maui.Graphics.Rect(face.X, face.Y, face.Width, face.Height)).ToArray();

            // Update the UI using Dispatcher.Dispatch
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                // Create a new FaceDetectionDrawable with the detected faces and a color
                CameraViewCanvas = new FaceDetectionDrawable(faceRectangles, Colors.Green, ProcessWidth, ProcessHeight);
            });
        }

        private void ProcessingSlam(byte[] imageData)
        {

        }

        // The method to process the image data
        public async Task GrabImageAsync(byte[] imageData, int width, int height)
        {
            byte[] clonedImageData = (byte[])imageData.Clone();

            // Handle the image data (e.g., display or process it)
            System.Diagnostics.Debug.WriteLine($"[ComputerVisionViewModel] Received image data with length: {clonedImageData.Length} {width}x{height}");

            if (_detector == "FaceDetection")
            {
                ProcessFaceDetection(clonedImageData);
            }
            else
            {
                ProcessingSlam(clonedImageData);
            }
        }

        public void GrabImageFromVideo(Mat Image)
        {
            Mat acquireImage = Image.Clone();

            KeyPoint[] keyPoints = ImageProcessing.FeatureExtraction(acquireImage);

            var points = new List<PointF>(keyPoints.Length);

            foreach (var keyPoint in keyPoints)
            {
                // Convert OpenCV KeyPoint to .NET PointF
                points.Add(new PointF(keyPoint.Pt.X, keyPoint.Pt.Y));
            }

            // Create a new FaceDetectionDrawable with the detected faces and a color
            CameraViewCanvas = new PointsDrawable(points);
        }
    }
}
