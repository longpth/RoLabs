namespace RLSharpSlam.MVVM.CustomViews;

public partial class DoubleBufferImage : ContentView
{
    private bool _isImage1Visible = true;

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
        if (_isImage1Visible)
        {
            Image2.Source = newSource;
        }
        else
        {
            Image1.Source = newSource;
        }

        _isImage1Visible = !_isImage1Visible;
    }
}