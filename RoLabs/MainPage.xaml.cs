using System.Diagnostics;
using Rolabs.MVVM.ViewModels;

namespace Rolabs
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
