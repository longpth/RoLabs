using Microsoft.Extensions.DependencyInjection;
using System;

namespace RLSharpSlam
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
