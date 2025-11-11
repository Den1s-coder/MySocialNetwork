using SocialNetwork.mobile.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    [QueryProperty(nameof(PostId), nameof(PostId))]
    public class PostDetailViewModel : BaseViewModel
    {
        private string postId;
        private string text;
        private string userName;
        private string newCommentText;

        public ObservableCollection<Comment> Comments { get; } = new ObservableCollection<Comment>();

        public Command AddCommentCommand { get; }

        public PostDetailViewModel()
        {
            AddCommentCommand = new Command(async () => await OnAddComment());
        }

        public string PostId
        {
            get => postId;
            set
            {
                postId = value;
                LoadPostId(value);
            }
        }

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string NewCommentText
        {
            get => newCommentText;
            set => SetProperty(ref newCommentText, value);
        }

        public async void LoadPostId(string id)
        {
            try
            {
                var api = DependencyService.Get<Services.ApiDataStore>();
                var post = await api.GetPostAsync(id);
                if (post == null)
                    return;

                Text = post.Text;
                UserName = post.UserName;
                Comments.Clear();
                foreach (var c in post.Comments)
                    Comments.Add(c);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task OnAddComment()
        {
            if (string.IsNullOrWhiteSpace(NewCommentText))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Comment cannot be empty", "OK");
                return;
            }

            try
            {
                var api = DependencyService.Get<Services.ApiDataStore>();
                var ok = await api.CreateCommentAsync(PostId, NewCommentText);
                if (ok)
                {
                    NewCommentText = string.Empty;
                    var post = await api.GetPostAsync(PostId);
                    Comments.Clear();
                    foreach (var c in post.Comments)
                        Comments.Add(c);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to add comment", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
