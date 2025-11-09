using SocialNetwork.mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class ProfilePostsViewModel : BaseViewModel
    {
        public ObservableCollection<Post> Posts { get; } = new ObservableCollection<Post>();
        public Command LoadMyPostsCommand { get; }

        public ProfilePostsViewModel()
        {
            Title = "My Posts";
            LoadMyPostsCommand = new Command(async () => await ExecuteLoadMyPostsCommand());
        }

        async Task ExecuteLoadMyPostsCommand()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Posts.Clear();
                var api = DependencyService.Get<Services.ApiDataStore>();
                var posts = await api.GetMyPostsAsync();
                foreach (var p in posts)
                    Posts.Add(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteLoadMyPostsCommand failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
