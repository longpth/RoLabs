using RoLabs.MVVM.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rolabs.MVVM.ViewModels
{
    public class ComputerVisionViewModel: BaseViewModel
    {
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
        }
    }
}
