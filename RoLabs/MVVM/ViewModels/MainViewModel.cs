using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rolabs.MVVM.ViewModels
{
    public class MainViewModel
    {
        public CameraViewModel CameraViewModel { get; }
        public ComputerVisionViewModel ComputerVisionViewModel { get; }

        public MainViewModel()
        {
            CameraViewModel = CameraViewModel.Instance;
            ComputerVisionViewModel = ComputerVisionViewModel.Instance;
        }
    }
}
