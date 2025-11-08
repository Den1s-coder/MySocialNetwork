using SocialNetwork.mobile.Models;
using SocialNetwork.mobile.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class ProfilePageViewModel : BaseViewModel
    {
        public ObservableCollection<Post> Posts { get; } = new ObservableCollection<Post>();

        private string userName;
        private string avatarUrl;
        private string bio;
        private Post _selectedPost;

        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string AvatarUrl
        {
            get => avatarUrl;
            set => SetProperty(ref avatarUrl, value);
        }

        public string Bio
        {
            get => bio;
            set => SetProperty(ref bio, value);
        }

        public Command LoadCommand { get; }
        public Command<Post> PostTapped { get; }

        public ProfilePageViewModel()
        {
            Title = "Profile";
            LoadCommand = new Command(async () => await ExecuteLoadCommand());
            PostTapped = new Command<Post>(OnPostSelected);
        }

        private async Task ExecuteLoadCommand()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Posts.Clear();
                var api = DependencyService.Get<Services.ApiDataStore>();
                var posts = await api.GetMyPostsAsync();
                if (posts != null)
                {
                    foreach (var p in posts)
                        Posts.Add(p);

                    // derive username and avatar from first post if available
                    var first = posts.GetEnumerator();
                    if (Posts.Count > 0)
                    {
                        UserName = Posts[0].UserName;
                        // AvatarUrl and Bio are not provided by posts; leave them empty or load profile endpoint if available
                    }
                }

                // If no posts or profile fields still empty, try to load profile details
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    try
                    {
                        var profileVm = new ProfileViewModel();
                        await profileVm.LoadProfileAsync();
                        UserName = profileVm.UserName;
                        AvatarUrl = profileVm.AvatarUrl;
                        Bio = profileVm.Bio;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Profile fallback load failed: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ProfilePageViewModel ExecuteLoadCommand failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public Post SelectedPost
        {
            get => _selectedPost;
            set
            {
                _selectedPost = value;
                OnPostSelected(value);
            }
        }

        async void OnPostSelected(Post post)
        {
            if (post == null) return;

            if (post.IsBanned)
                return; // don't navigate to banned posts

            await Shell.Current.GoToAsync($"{nameof(PostDetailPage)}?{nameof(PostDetailViewModel.PostId)}={post.Id}");
        }
    }
}
