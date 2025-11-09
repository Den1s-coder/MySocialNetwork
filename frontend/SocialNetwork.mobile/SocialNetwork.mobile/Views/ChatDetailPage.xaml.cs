using SocialNetwork.mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SocialNetwork.mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatDetailPage : ContentPage
    {
        ChatDetailViewModel _vm;
        public ChatDetailPage()
        {
            InitializeComponent();
            BindingContext = _vm = new ChatDetailViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_vm == null) return;
            await _vm.InitializeConnectionAsync();
            await _vm.LoadMessages();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            // TODO: stop connection
        }
    }
}
