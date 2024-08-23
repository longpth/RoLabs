using Rolabs.MVVM.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rolabs.MVVM.ViewModels
{
    public class VisionSizeViewModel : BaseViewModel
    {
        public int VisionWidth => Global.VisionWidth;
        public int VisionHeight => Global.VisionHeight;

        private static VisionSizeViewModel _instance;
        private static readonly object _lock = new object();

        // Private constructor to prevent direct instantiation
        private VisionSizeViewModel() { }

        // Public static property to get the singleton instance
        public static VisionSizeViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new VisionSizeViewModel();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
