using SocialNetwork.mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SocialNetwork.mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        ProfilePageViewModel _vm;
        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = _vm = new ProfilePageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_vm.LoadCommand.CanExecute(null))
                _vm.LoadCommand.Execute(null);
        }
    }
}