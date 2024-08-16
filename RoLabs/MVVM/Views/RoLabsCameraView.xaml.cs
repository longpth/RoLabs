using Rolabs.MVVM.ViewModels;

namespace Rolabs.MVVM.Views;

public partial class RoLabsCameraView : ContentView
{
    private MainViewModel _mainViewModel = new MainViewModel();

    public RoLabsCameraView()
    {
        InitializeComponent();

        BindingContext = _mainViewModel;
        cameraView.RegisterImageGrabbedCallback(_mainViewModel.CameraViewModel.GrabImage);
        cameraView.RegisterImageGrabbedCallback(_mainViewModel.ComputerVisionViewModel.GrabImage);
    }
}