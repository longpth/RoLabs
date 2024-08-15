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
    private MainViewModel _mainViewModel = new MainViewModel();

    public CameraView()
    {
        InitializeComponent();
        BindingContext = _mainViewModel;
        cameraView.RegisterImageGrabbedCallback(_mainViewModel.CameraViewModel.GrabImage);
        cameraView.RegisterImageGrabbedCallback(_mainViewModel.ComputerVisionViewModel.GrabImage);
    }
}