using Rolabs.MVVM.ViewModels;

namespace Rolabs.MVVM.Views;

public partial class RoLabsCameraView : ContentView
{
    private MainVisionViewModel _mainVisionViewModel = new MainVisionViewModel();

    public RoLabsCameraView()
    {
        InitializeComponent();

        BindingContext = _mainVisionViewModel;
        cameraView.RegisterImageGrabbedCallback(_mainVisionViewModel.CameraViewModel.GrabImageAsync);
        cameraView.RegisterImageGrabbedCallback(_mainVisionViewModel.ComputerVisionViewModel.GrabImageAsync);
    }
}