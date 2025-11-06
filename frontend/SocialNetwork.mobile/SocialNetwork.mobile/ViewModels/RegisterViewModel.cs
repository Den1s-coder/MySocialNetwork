using SocialNetwork.mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string username;
        private string email;
        private string password;

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public Command RegisterCommand { get; }
        public Command CancelCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(OnRegisterClicked);
            CancelCommand = new Command(OnCancelClicked);
        }

        private async void OnRegisterClicked(object obj)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please fill all fields", "OK");
                return;
            }

            // TODO: call registration API and handle errors. For now show success and navigate to main page.
            await Application.Current.MainPage.DisplayAlert("Success", "Account created", "OK");
            await Shell.Current.GoToAsync("//ItemsPage");
        }

        private async void OnCancelClicked(object obj)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
