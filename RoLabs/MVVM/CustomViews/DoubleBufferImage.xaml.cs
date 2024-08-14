namespace Rolabs.MVVM.CustomViews;

public partial class DoubleBufferImage : ContentView
{
    private int _imageCnt = 0;

    public static readonly BindableProperty ImageSourceProperty =
        BindableProperty.Create(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(DoubleBufferImage),
            default(ImageSource),
            propertyChanged: OnImageSourceChanged);

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public DoubleBufferImage()
    {
        InitializeComponent();
    }

    private static async void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DoubleBufferImage)bindable;
        await control.UpdateImageSourceAsync((ImageSource)newValue);
    }

    public async Task UpdateImageSourceAsync(ImageSource newSource)
    {
        switch (_imageCnt)
        {
            case 0:
                Image2.IsVisible = true;
                Image1.IsVisible = false;
                Image1.Source = newSource;
                _imageCnt++;
                break;
            case 1:
                Image3.IsVisible = true;
                Image2.IsVisible = false;
                Image2.Source = newSource;
                _imageCnt++;
                break;
            case 2:
                Image1.IsVisible = true;
                Image3.IsVisible = false;
                Image3.Source = newSource;
                _imageCnt = 0;
                break;
            default:
                break;
        }
    }
}