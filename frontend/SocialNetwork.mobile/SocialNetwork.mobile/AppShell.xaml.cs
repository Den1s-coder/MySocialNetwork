using SocialNetwork.mobile.Views;
using System;
using Xamarin.Forms;

namespace SocialNetwork.mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(PostsPage), typeof(PostsPage));
            Routing.RegisterRoute(nameof(PostDetailPage), typeof(PostDetailPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
