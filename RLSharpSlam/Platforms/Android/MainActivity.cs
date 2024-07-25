using Android.App;
using Android.Content.PM;
using Android.OS;
using MediaManager;

namespace RLSharpSlam
{
    [Activity(Theme = "@style/Maui.SplashTheme", ResizeableActivity = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    CrossMediaManager.Current.Init(this);
        //}
    }
}
