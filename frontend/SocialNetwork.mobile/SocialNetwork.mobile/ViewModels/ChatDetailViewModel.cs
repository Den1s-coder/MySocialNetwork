using SocialNetwork.mobile.DTO;
using SocialNetwork.mobile.Services;
using System;
using MvvmHelpers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Essentials;
using System.Linq;

namespace SocialNetwork.mobile.ViewModels
{
    [QueryProperty(nameof(ChatId), nameof(ChatId))]
    public class ChatDetailViewModel : BaseViewModel
    {
        public ObservableRangeCollection<MessageDto> Messages { get; } = new ObservableRangeCollection<MessageDto>();
        public string ChatId { get; set; }
        public Command SendCommand { get; }
        private string newMessage;
        private HubConnection _connection;

        public Guid CurrentUserId { get; private set; } = Guid.Empty;

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
            try
            {
                var userIdStr = await SecureStorage.GetAsync("user_id");
                Debug.WriteLine($"InitializeConnectionAsync: user_id raw='{userIdStr}'");
                if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out var uid))
                {
                    CurrentUserId = uid;
                    Debug.WriteLine($"InitializeConnectionAsync: CurrentUserId = {CurrentUserId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeConnectionAsync: cannot read user_id from SecureStorage: {ex}");
            }

            var client = new ChatServiceClient();
            _connection = client.CreateHubConnection(async () => await SecureStorage.GetAsync("auth_token"));

            _connection.Reconnecting += (ex) =>
            {
                Debug.WriteLine($"SignalR: Reconnecting: {ex}");
                return Task.CompletedTask;
            };
            _connection.Reconnected += (id) =>
            {
                Debug.WriteLine($"SignalR: Reconnected, connectionId={id}");
                return Task.CompletedTask;
            };
            _connection.Closed += (ex) =>
            {
                Debug.WriteLine($"SignalR: Closed: {ex}");
                return Task.CompletedTask;
            };

            _connection.On<MessageDto>("ReceiveMessage", (msg) =>
            {
                Device.BeginInvokeOnMainThread(() => InsertMessageSorted(msg));
            });

            try
            {
                await _connection.StartAsync();

                if (CurrentUserId != Guid.Empty && !string.IsNullOrWhiteSpace(ChatId))
                {
                    await _connection.InvokeAsync("JoinChat", Guid.Parse(ChatId), CurrentUserId);
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
                var msgs = (await client.GetMessagesAsync(Guid.Parse(ChatId))).ToList();

                var sorted = msgs.OrderBy(m => m.SentAt).ToList();
                foreach (var m in sorted)
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

        private void InsertMessageSorted(MessageDto msg)
        {
            if (msg == null) return;

            if (Messages.Count == 0)
            {
                Messages.Add(msg);
                return;
            }

            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                if (Messages[i].SentAt <= msg.SentAt)
                {
                    Messages.Insert(i + 1, msg);
                    return;
                }
            }
            Messages.Insert(0, msg);
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (_connection == null) return false;

            if (_connection.State == HubConnectionState.Connected)
                return true;

            if (_connection.State == HubConnectionState.Connecting || _connection.State == HubConnectionState.Reconnecting)
            {
                var wait = Task.Delay(5000);
                while ((_connection.State == HubConnectionState.Connecting || _connection.State == HubConnectionState.Reconnecting) && !wait.IsCompleted)
                {
                    await Task.Delay(100);
                }
                return _connection.State == HubConnectionState.Connected;
            }

            try
            {
                await _connection.StartAsync();
                return _connection.State == HubConnectionState.Connected;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureConnectedAsync: StartAsync failed: {ex}");
                return false;
            }
        }

        private async Task Send()
        {
            if (string.IsNullOrWhiteSpace(NewMessage)) return;

            try
            {
                var connected = await EnsureConnectedAsync();
                if (!connected)
                {
                    Debug.WriteLine("Send aborted: SignalR connection is not active.");
                    return;
                }

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
