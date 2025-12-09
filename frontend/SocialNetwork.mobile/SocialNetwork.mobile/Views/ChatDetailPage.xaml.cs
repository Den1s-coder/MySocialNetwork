using SocialNetwork.mobile.ViewModels;
using SocialNetwork.mobile.Selectors;
using System;
using System.Diagnostics;
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

            try
            {
                if (Resources.ContainsKey("MessageTemplateSelector") && Resources["MessageTemplateSelector"] is MessageTemplateSelector selector)
                {
                    selector.CurrentUserId = _vm.CurrentUserId;
                    Debug.WriteLine($"ChatDetailPage: selector.CurrentUserId = {selector.CurrentUserId}");
                }
                else
                {
                    Debug.WriteLine("ChatDetailPage: MessageTemplateSelector resource not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set template selector user id: {ex}");
            }

            await _vm.LoadMessages();

            if (_vm.Messages.Count > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagesList.ScrollTo(
                        item: _vm.Messages[_vm.Messages.Count - 1], 
                        position: ScrollToPosition.End,            
                        animate: false                             
                    );
                });
            }
        }
    }
}
