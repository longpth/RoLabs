using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Rolabs.MVVM.CustomViews
{
    public partial class MicToggleButton : ContentView
    {
        public static readonly BindableProperty ButtonWidthRequestProperty = BindableProperty.Create(
            propertyName: nameof(ButtonWidthRequest),
            returnType: typeof(double),
            declaringType: typeof(MicToggleButton),
            defaultValue: 100.0);

        public static readonly BindableProperty ButtonHeightRequestProperty = BindableProperty.Create(
            propertyName: nameof(ButtonHeightRequest),
            returnType: typeof(double),
            declaringType: typeof(MicToggleButton),
            defaultValue: 100.0);

        public double ButtonWidthRequest
        {
            get => (double)GetValue(ButtonWidthRequestProperty);
            set => SetValue(ButtonWidthRequestProperty, value);
        }

        public double ButtonHeightRequest
        {
            get => (double)GetValue(ButtonHeightRequestProperty);
            set => SetValue(ButtonHeightRequestProperty, value);
        }

        public static readonly BindableProperty IsListeningProperty = BindableProperty.Create(
            propertyName: nameof(IsListening),
            returnType: typeof(bool),
            declaringType: typeof(MicToggleButton),
            defaultValue: false,
            propertyChanged: OnIsListeningChanged);

        public bool IsListening
        {
            get => (bool)GetValue(IsListeningProperty);
            set => SetValue(IsListeningProperty, value);
        }

        public MicToggleButton()
        {
            InitializeComponent();
            UpdateMicButton();
        }

        private void OnMicButtonClicked(object sender, EventArgs e)
        {
            IsListening = !IsListening;
        }

        private static void OnIsListeningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MicToggleButton)bindable;
            control.UpdateMicButton();
        }

        private void UpdateMicButton()
        {
            MicButton.Source = IsListening ? "microphone_on.png" : "microphone_off.png";
            MicButton.WidthRequest = ButtonWidthRequest;
            MicButton.HeightRequest = ButtonHeightRequest;
        }
    }
}
