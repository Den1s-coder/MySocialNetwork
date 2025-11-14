using SocialNetwork.mobile.Models;
using SocialNetwork.mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

                // 1) Попытаться тихо загрузить профиль — чтобы всегда иметь UserName, даже если постов нет
                try
                {
                    var profileVm = new ProfileViewModel();
                    var profileOk = await profileVm.TryLoadProfileAsync();
                    if (profileOk)
                    {
                        UserName = profileVm.UserName;
                        AvatarUrl = profileVm.AvatarUrl;
                        Bio = profileVm.Bio;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Profile quick load failed: {ex}");
                }

                // 2) Загрузить посты
                var api = DependencyService.Get<Services.ApiDataStore>();
                var posts = await api.GetMyPostsAsync() ?? Enumerable.Empty<Post>();

                foreach (var p in posts)
                    Posts.Add(p);

                // 3) Если профиль не заполнился, взять имя из первого поста (если есть)
                var firstPost = Posts.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(UserName) && firstPost != null)
                {
                    UserName = firstPost.UserName;
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
                if (SetProperty(ref _selectedPost, value) && value != null)
                {
                    OnPostSelected(value);
                }
            }
        }

        async void OnPostSelected(Post post)
        {
            if (post == null) return;

            if (post.IsBanned)
                return; 

            await Shell.Current.GoToAsync($"{nameof(PostDetailPage)}?{nameof(PostDetailViewModel.PostId)}={post.Id}");
        }
    }
}

