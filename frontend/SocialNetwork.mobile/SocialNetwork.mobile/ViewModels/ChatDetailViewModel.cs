using SocialNetwork.mobile.DTO;
using SocialNetwork.mobile.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Essentials;

namespace SocialNetwork.mobile.ViewModels
{
    [QueryProperty(nameof(ChatId), nameof(ChatId))]
    public class ChatDetailViewModel : BaseViewModel
    {
        public ObservableCollection<MessageDto> Messages { get; } = new ObservableCollection<MessageDto>();
        public string ChatId { get; set; }
        public Command SendCommand { get; }
        private string newMessage;
        private HubConnection _connection;

        public string NewMessage
        {
            get => newMessage;
            set => SetProperty(ref newMessage, value);
        }

        public ChatDetailViewModel()
        {
            SendCommand = new Command(async () => await Send());
        }

        public async Task InitializeConnectionAsync()
        {
            var client = new ChatServiceClient();
            _connection = client.CreateHubConnection(async () => await SecureStorage.GetAsync("auth_token"));
            _connection.On<MessageDto>("ReceiveMessage", (msg) =>
            {
                Device.BeginInvokeOnMainThread(() => Messages.Add(msg));
            });

            try
            {
                await _connection.StartAsync();
                // join group
                var userIdStr = await SecureStorage.GetAsync("user_id");
                if (!string.IsNullOrWhiteSpace(userIdStr) && !string.IsNullOrWhiteSpace(ChatId))
                {
                    Guid userId;
                    if (Guid.TryParse(userIdStr, out userId))
                    {
                        await _connection.InvokeAsync("JoinChat", Guid.Parse(ChatId), userId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeConnectionAsync failed: {ex}");
            }
        }

        public async Task LoadMessages()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Messages.Clear();
                var client = new ChatServiceClient();
                var msgs = await client.GetMessagesAsync(Guid.Parse(ChatId));
                foreach (var m in msgs)
                    Messages.Add(m);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadMessages failed: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task Send()
        {
            if (string.IsNullOrWhiteSpace(NewMessage)) return;
            try
            {
                await _connection.InvokeAsync("SendMessage", Guid.Parse(ChatId), NewMessage);
                NewMessage = string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Send failed: {ex}");
            }
        }
    }
}
