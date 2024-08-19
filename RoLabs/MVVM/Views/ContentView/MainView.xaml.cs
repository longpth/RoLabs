using Android.Media;
using Plugin.Maui.Audio;
using Rolabs.MVVM.ViewModels;

namespace Rolabs.MVVM.Views;

public partial class MainView : ContentView
{
    public MainView()
	{
		InitializeComponent();

        BindingContext = MainViewViewModel.Instance;
    }
}