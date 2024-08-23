using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rolabs.MVVM.ViewModels
{
    public class MainVisionViewModel
    {
        public CameraViewModel CameraViewModel { get; }
        public ComputerVisionViewModel ComputerVisionViewModel { get; }

        public VisionSizeViewModel VisionSizeViewModel { get; }

        public MainVisionViewModel()
        {
            CameraViewModel = CameraViewModel.Instance;
            ComputerVisionViewModel = ComputerVisionViewModel.Instance;
            VisionSizeViewModel = VisionSizeViewModel.Instance;
        }
    }
}
