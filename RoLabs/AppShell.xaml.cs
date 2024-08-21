using Rolabs.MVVM.Views;

namespace Rolabs
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(VisionPage), typeof(VisionPage));

        }
    }
}
