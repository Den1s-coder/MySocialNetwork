using SocialNetwork.mobile.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace SocialNetwork.mobile.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}