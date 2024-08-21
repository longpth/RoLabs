namespace Rolabs.MVVM.Views;

public partial class VisionPage : ContentPage
{
    public VisionPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        // Navigate back to MainPage explicitly
        Shell.Current.GoToAsync(nameof(MainPage));
        return true; // Prevent default behavior
    }
}