using Android.Content.Res;
using Com.Google.Android.Exoplayer2.UI;
using CommunityToolkit.Maui.Views;
using Java.Net;
using MediaManager;
using MediaManager.Forms;
using OpenCvSharp;
using RLSharpSlam.MVVM.ViewModels;

namespace RLSharpSlam.MVVM.Views;

public partial class CameraView : ContentView
{
    private CameraViewModel _cameraViewModel;

    public CameraView()
    {
        InitializeComponent();
        _cameraViewModel = CameraViewModel.Instance;
        BindingContext = _cameraViewModel;
    }
}