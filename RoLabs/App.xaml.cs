using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rolabs
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
