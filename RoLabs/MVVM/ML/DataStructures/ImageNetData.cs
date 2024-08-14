using Android.Graphics;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace RoLabs.MVVM.ML.DataStructure
{
    public class ImageNetData
    {
        // Dimensions provided here seem not to play an important role
        [ImageType(480, 640)]
        public Bitmap InputImage { get; set; }

        public string Label { get; set; }

        public ImageNetData()
        {
            InputImage = null;
            Label = "";
        }
    }
}
