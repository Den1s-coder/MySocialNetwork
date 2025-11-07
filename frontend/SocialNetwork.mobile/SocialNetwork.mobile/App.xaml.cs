using SocialNetwork.mobile.Services;
using SocialNetwork.mobile.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SocialNetwork.mobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            // Use ApiDataStore by default (backend-backed). Keep MockDataStore available if needed.
            DependencyService.Register<ApiDataStore>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
