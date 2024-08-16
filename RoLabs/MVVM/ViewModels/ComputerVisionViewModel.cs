using RoLabs.MVVM.Services;
using OpenCvSharp;
using Rolabs.MVVM.Helpers;
using Rolabs.MVVM.CustomViews;

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

        // The method to process the image data
        public void GrabImage(byte[] imageData, int width, int height)
        {
            // Handle the image data (e.g., display or process it)
            System.Diagnostics.Debug.WriteLine($"[ComputerVisionViewModel] Received image data with length: {imageData.Length} {width}x{height}");

            //var boundingBoxes = ObjectDetection.Instance.Score(imageData);
            //System.Diagnostics.Debug.WriteLine($"[ComputerVisionViewModel] boundingBoxes count = {boundingBoxes.Count}");

            (OpenCvSharp.Rect[] detectedFaces, OpenCvSharp.Mat tmp) = FaceDetection.Instance.DetectFaces(imageData);

            //Convert OpenCvSharp.Rect to Microsoft.Maui.Graphics.Rect for drawing
            var faceRectangles = detectedFaces.Select(face => new Microsoft.Maui.Graphics.Rect(face.X, face.Y, face.Width, face.Height)).ToArray();

            //Create a new FaceDetectionDrawable with the detected faces and a color, the image is transposed
            CameraViewCanvas = new FaceDetectionDrawable(faceRectangles, Colors.Green, ProcessWidth, ProcessHeight);
        }
    }
}
