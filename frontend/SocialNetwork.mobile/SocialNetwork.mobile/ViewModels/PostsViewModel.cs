using SocialNetwork.mobile.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using SocialNetwork.mobile.Views;

namespace SocialNetwork.mobile.ViewModels
{
    public class PostsViewModel : BaseViewModel
    {
        private Post _selectedPost;
        private string _newPostText;

        public ObservableCollection<Post> Posts { get; }
        public Command LoadPostsCommand { get; }
        public Command<Post> PostTapped { get; }
        public Command CreatePostCommand { get; }

        public string NewPostText { 
            get => _newPostText; 
            set => SetProperty(ref _newPostText, value); 
        }

        public PostsViewModel()
        {
            Title = "Posts";
            Posts = new ObservableCollection<Post>();
            LoadPostsCommand = new Command(async () => await ExecuteLoadPostsCommand());
            PostTapped = new Command<Post>(OnPostSelected);
            CreatePostCommand = new Command(async () => await OnCreatePost());
        }

        async Task ExecuteLoadPostsCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                Posts.Clear();
                var api = DependencyService.Get<Services.ApiDataStore>();

                var posts = await api.GetPostsAsync();
 
                if (posts != null)
                {
                    int count = 0;
                    foreach (var post in posts)
                    {
                        Posts.Add(post);
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteLoadPostsCommand exception: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("ExecuteLoadPostsCommand finished");
            }
        }

        public void OnAppearing()
        {
            SelectedPost = null;
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
            if (post == null)
                return;

            await Shell.Current.GoToAsync($"{nameof(PostDetailPage)}?{nameof(PostDetailViewModel.PostId)}={post.Id}");
        }

        private async Task OnCreatePost()
        {
            if (string.IsNullOrWhiteSpace(NewPostText))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Post text cannot be empty.", "OK");
                return;
            }

            try
            {
                var api = DependencyService.Get<Services.ApiDataStore>();
                var item = new Item { Text = NewPostText };
                var success = await api.AddItemAsync(item);
                if (success)
                {
                    NewPostText = string.Empty;
                    await ExecuteLoadPostsCommand();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to create post.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnCreatePost exception: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", "An error occurred while creating the post.", "OK");
            }
        }
    }
}
