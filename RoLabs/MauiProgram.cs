
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rolabs.MVVM.ViewModels;
using Camera.MAUI;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using MediaManager.Platforms.Android.Video;
using Plugin.Maui.Audio;
using Android.Media;

namespace Rolabs
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCameraView()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCompatibility()
                        .ConfigureMauiHandlers((handlers) => {
#if ANDROID
                            handlers.AddCompatibilityRenderer(typeof(VideoView), typeof(MediaManager.Forms.Platforms.Android.VideoViewRenderer));
#endif

#if IOS
                            handlers.AddCompatibilityRenderer(typeof(VideoView), typeof(MediaManager.Forms.Platforms.iOS.VideoViewRenderer));
#endif
                        });

            builder.Services.AddSingleton(Plugin.Maui.Audio.AudioManager.Current);
            builder.Services.AddTransient<MainPage>();

            var app = builder.Build();

            return app;
        }
    }
}
