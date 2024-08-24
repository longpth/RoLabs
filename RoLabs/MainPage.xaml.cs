#if ANDROID
using System.Diagnostics;
using Android.Media;
#endif
using Plugin.Maui.Audio;
using Rolabs.MVVM.ViewModels;
using Rolabs.MVVM.Views;

namespace Rolabs
{
    public partial class MainPage : ContentPage
    {

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            MainViewViewModel.Instance.AudioManager = audioManager;
            MainViewViewModel.Instance.AudioRecorder = audioManager.CreateRecorder();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //CameraViewModel.Instance.Dispose();
        }

        protected override void OnDisappearing() 
        { 
            base.OnDisappearing();
            CameraViewModel.Instance.Dispose();
        }

        private async void OnVisionButtonClicked(object sender, EventArgs e)
        {
            // Navigate to the VisionPage
            await Shell.Current.GoToAsync(nameof(VisionPage));
        }

    }
}
