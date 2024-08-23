using RoLabs.MVVM.Services;
using OpenCvSharp;
using RoLabsSlamSharp;
using Rolabs.MVVM.CustomViews;
using System.Collections.ObjectModel;
using Rolabs.MVVM.Helpers;

namespace Rolabs.MVVM.ViewModels
{
    public class ComputerVisionViewModel: BaseViewModel
    {

        private ImageSource _imageSource;

        private RolabsSlamSharpWrapper _rolabsSlamSharpWrapper = new RolabsSlamSharpWrapper();

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

        private bool _cameraRunning = false;

        public bool CameraRunning
        {
            get { return _cameraRunning; }
            set
            {
                if (_cameraRunning != value)
                {
                    _cameraRunning = value;
                    if(_cameraRunning)
                    {
                        _rolabsSlamSharpWrapper.SetCameraIntrinsics(458.654f, 457.296f, 367.215f, 248.375f);
                        _rolabsSlamSharpWrapper.Start();
                    }
                    else
                    {
                        _rolabsSlamSharpWrapper.Stop();
                    }
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
                CameraViewCanvas = new FaceDetectionDrawable(faceRectangles, Colors.Green, Global.VisionWidth, Global.VisionHeight);
            });
        }

        private void ProcessingSlam(byte[] imageData)
        {

        }

        private void ProcessingSlam(Mat image)
        {
            if (!image.Empty())
            {
                Mat acquireImage = image.Clone();

#if false
                KeyPoint[] keyPoints = ImageProcessing.FeatureExtraction(acquireImage);

#else
                Mat gray = new Mat();

                // Convert image to grayscale
                Cv2.CvtColor(acquireImage, gray, ColorConversionCodes.BGR2GRAY);

                //string cacheFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsoluteFile.Path.ToString();
                //cacheFolder = cacheFolder + System.IO.Path.DirectorySeparatorChar;
                //var fileNameToSave = cacheFolder + "testcppwrapper.jpg";
                //Cv2.ImWrite(fileNameToSave, testImage);

                //KeyPoint[] keyPoints = _rolabsSlamSharpWrapper.RoLabsFeatureExtraction(gray);

                _rolabsSlamSharpWrapper.GrabImage(gray);
                KeyPoint[] keyPoints = _rolabsSlamSharpWrapper.GetDebugKeyPoints();
#endif

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
            if (_detector == "FaceDetection")
            {
                // TODO
            }
            else
            {
                ProcessingSlam(Image);
            }
        }
    }
}
