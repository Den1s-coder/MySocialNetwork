using SocialNetwork.mobile.DTO;
using SocialNetwork.mobile.Services;
using SocialNetwork.mobile.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class ChatsViewModel : BaseViewModel
    {
        public ObservableCollection<ChatDto> Chats { get; } = new ObservableCollection<ChatDto>();
        public Command LoadChatsCommand { get; }

        private ChatDto _selectedChat;
        public ChatDto SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (SetProperty(ref _selectedChat, value))
                {
                    OnChatSelected(value);
                }
            }
        }

        private int chatsCount;
        public int ChatsCount
        {
            get => chatsCount;
            set => SetProperty(ref chatsCount, value);
        }

        private bool hasChats;
        public bool HasChats
        {
            get => hasChats;
            set => SetProperty(ref hasChats, value);
        }

        public ChatsViewModel()
        {
            Title = "Chats";
            LoadChatsCommand = new Command(async () => await ExecuteLoadChats());
        }

        private async Task ExecuteLoadChats()
        {
            Debug.WriteLine("ExecuteLoadChats start");
            if (IsBusy) {
                Debug.WriteLine("ExecuteLoadChats: IsBusy, returning");
                return;
            }
            IsBusy = true;
            try
            {
                Chats.Clear();
                HasChats = false;
                ChatsCount = 0;

                var client = new ChatServiceClient();
                Debug.WriteLine("Calling GetMyChatsAsync");
                var chats = await client.GetMyChatsAsync();

                int returnedCount = -1;
                if (chats == null) returnedCount = 0;
                else if (chats is System.Collections.ICollection col) returnedCount = col.Count;
                Debug.WriteLine($"GetMyChatsAsync returned {returnedCount} items");

                if (chats != null)
                {
                    // Ensure UI updates happen on main thread
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var c in chats)
                            Chats.Add(c);

                        ChatsCount = Chats.Count;
                        HasChats = ChatsCount > 0;
                        Debug.WriteLine($"Added {Chats.Count} chats to collection (UI thread)");
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ExecuteLoadChats failed: {ex}");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("ExecuteLoadChats finished");
            }
        }

        private async void OnChatSelected(ChatDto chat)
        {
            if (chat == null) return;

            try
            {
                // Navigate to ChatDetailPage with chat id using registered route name
                await Shell.Current.GoToAsync($"{nameof(ChatDetailPage)}?ChatId={chat.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation to chat failed: {ex}");
            }
        }
    }
}
