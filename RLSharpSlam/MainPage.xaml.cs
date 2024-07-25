using System.Diagnostics;
using RLSharpSlam.MVVM.ViewModels;

namespace RLSharpSlam
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CameraViewModel.Instance.Dispose();
        }
    }
}
