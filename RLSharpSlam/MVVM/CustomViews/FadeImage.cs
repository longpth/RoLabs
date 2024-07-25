using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RLSharpSlam.MVVM.CustomViews
{
    public class FadeImage: Image
    {
        public static readonly BindableProperty CustomImageSourceProperty =
    BindableProperty.Create(
        nameof(CustomImageSource),
        typeof(ImageSource),
        typeof(FadeImage),
        default(ImageSource),
        propertyChanged: OnCustomImageSourceChanged);

        public ImageSource CustomImageSource
        {
            get => (ImageSource)GetValue(CustomImageSourceProperty);
            set => SetValue(CustomImageSourceProperty, value);
        }

        private static async void OnCustomImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var fadeImage = bindable as FadeImage;
            if (fadeImage != null)
            {
                await fadeImage.FadeOutAndChangeSourceAsync((ImageSource)newValue);
            }
        }

        private async Task FadeOutAndChangeSourceAsync(ImageSource newSource)
        {
            await this.FadeTo(0, 250);
            this.Source = newSource;
            await this.FadeTo(1, 250);
        }
    }
}

