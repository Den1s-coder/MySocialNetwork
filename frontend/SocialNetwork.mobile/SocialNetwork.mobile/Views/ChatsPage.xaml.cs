using SocialNetwork.mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SocialNetwork.mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatsPage : ContentPage
    {
        ChatsViewModel _vm;
        public ChatsPage()
        {
            InitializeComponent();
            BindingContext = _vm = new ChatsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_vm.LoadChatsCommand.CanExecute(null))
                _vm.LoadChatsCommand.Execute(null);
        }
    }
}
