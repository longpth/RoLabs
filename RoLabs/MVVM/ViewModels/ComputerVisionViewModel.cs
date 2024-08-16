using RoLabs.MVVM.Services;
using OpenCvSharp;
using Rolabs.MVVM.Helpers;

namespace Rolabs.MVVM.ViewModels
{
    public class ComputerVisionViewModel: BaseViewModel
    {
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

            Mat result = FaceDetection.Instance.DetectFaces(imageData);
            ImageSource = result.ToImageSource();
        }
    }
}
