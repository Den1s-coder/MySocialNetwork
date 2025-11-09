using SocialNetwork.mobile.ViewModels;
using SocialNetwork.mobile.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SocialNetwork.mobile
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(PostsPage), typeof(PostsPage));
            Routing.RegisterRoute(nameof(PostDetailPage), typeof(PostDetailPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(ChatsPage), typeof(ChatsPage));
            Routing.RegisterRoute(nameof(ChatDetailPage), typeof(ChatDetailPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
