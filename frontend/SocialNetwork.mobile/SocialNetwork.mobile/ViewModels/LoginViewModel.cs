using SocialNetwork.mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string username;
        private string password;

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            RegisterCommand = new Command(OnRegisterClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            // TODO: perform real authentication here (call API, validate token, store credentials)
            // For now accept any non-empty credentials and navigate to main posts page
            await Shell.Current.GoToAsync("//ItemsPage");
        }

        private async void OnRegisterClicked(object obj)
        {
            // Navigate to register page
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}
