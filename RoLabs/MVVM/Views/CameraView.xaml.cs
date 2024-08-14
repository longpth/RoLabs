using Android.Content.Res;
using Com.Google.Android.Exoplayer2.UI;
using CommunityToolkit.Maui.Views;
using Java.Net;
using MediaManager;
using MediaManager.Forms;
using OpenCvSharp;
using Rolabs.MVVM.ViewModels;

namespace Rolabs.MVVM.Views;

public partial class CameraView : ContentView
{
    private CameraViewModel _cameraViewModel;

    public CameraView()
    {
        InitializeComponent();
        _cameraViewModel = CameraViewModel.Instance;
        BindingContext = _cameraViewModel;
        cameraView.ImageCallback = ProcessImage;
    }

    // The method to process the image data
    private void ProcessImage(byte[] imageData, int width, int height)
    {
        // Handle the image data (e.g., display or process it)
        System.Diagnostics.Debug.WriteLine($"Received image data with length: {imageData.Length} {width}x{height}");
        _cameraViewModel.CameraGrabbedImage = Mat.FromImageData(imageData, ImreadModes.Color);
    }

}