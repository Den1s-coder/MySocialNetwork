using SocialNetwork.mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SocialNetwork.mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PostsPage : ContentPage
    {
        PostsViewModel _viewModel;

        public PostsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new PostsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();

            // Ensure posts are loaded when the page appears
            if (_viewModel.LoadPostsCommand?.CanExecute(null) ?? false)
            {
                _viewModel.LoadPostsCommand.Execute(null);
            }
        }
    }
}
