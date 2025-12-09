using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SocialNetwork.mobile.Models;
using SocialNetwork.mobile.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Linq;

namespace SocialNetwork.mobile.Services
{
    public class ChatServiceClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private const string TokenKey = "auth_token";

        public ChatServiceClient(string baseUrl = "https://10.0.2.2:7142")
        {
            _baseUrl = baseUrl;
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _client = new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
        }

        private async Task SetAuthHeader()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<IEnumerable<ChatDto>> GetMyChatsAsync()
        {
            await SetAuthHeader();
            try
            {
                Debug.WriteLine($"GetMyChatsAsync calling {_client.BaseAddress}/api/chat/chats");
                var resp = await _client.GetAsync("/api/chat/chats");
                Debug.WriteLine($"GetMyChatsAsync response status: {resp.StatusCode}");
                var body = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"GetMyChatsAsync response body length: {body?.Length}");
                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"GetMyChatsAsync non-success body: {body}");
                    return new List<ChatDto>();
                }

                var chats = JsonConvert.DeserializeObject<List<ChatDto>>(body);

                var myUserIdStr = await SecureStorage.GetAsync("user_id");
                Guid myUserId;
                Guid.TryParse(myUserIdStr, out myUserId);

                if (chats != null)
                {
                    foreach (var chat in chats)
                    {
                        var names = new List<string>();
                        if (chat.UserChats != null)
                        {
                            foreach (var uc in chat.UserChats)
                            {
                                if (!string.IsNullOrWhiteSpace(uc.UserName))
                                    names.Add(uc.UserName);
                                else if (uc.UserId != Guid.Empty)
                                    names.Add(uc.UserId.ToString());
                                else if (!string.IsNullOrWhiteSpace(uc.Role))
                                    names.Add(uc.Role);
                                else
                                    names.Add("User");
                            }
                        }

                        chat.ParticipantNames = names;

                        bool isPrivate = false;
                        if (!string.IsNullOrWhiteSpace(chat.Type))
                        {
                            if (chat.Type.Equals("Private", StringComparison.OrdinalIgnoreCase))
                                isPrivate = true;
                            else
                            {
                                if (int.TryParse(chat.Type, out var tval) && tval == 0)
                                    isPrivate = true;
                            }
                        }

                        if (isPrivate)
                        {                            
                            var others = names.Where(n => n != myUserId.ToString()).ToList();
                            chat.DisplayTitle = others.Any() ? string.Join(", ", others) : (names.FirstOrDefault() ?? "Chat");
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(chat.Title))
                            {
                                var others = names.Where(n => n != myUserId.ToString()).ToList();
                                chat.DisplayTitle = others.Any() ? string.Join(", ", others) : (names.FirstOrDefault() ?? "Chat");
                            }
                            else
                            {
                                chat.DisplayTitle = chat.Title;
                            }
                        }
                    }
                }

                Debug.WriteLine($"GetMyChatsAsync parsed {chats?.Count ?? 0} chats");
                return chats ?? new List<ChatDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMyChatsAsync failed: {ex}");
                return new List<ChatDto>();
            }
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatId)
        {
            await SetAuthHeader();
            try
            {
                var resp = await _client.GetAsync($"/api/chat/chats/{chatId}/messages");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                var messages = JsonConvert.DeserializeObject<List<MessageDto>>(json);
                return messages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetMessagesAsync failed: {ex}");
                return new List<MessageDto>();
            }
        }

        public HubConnection CreateHubConnection(Func<Task<string>> getToken)
        {
            var conn = new HubConnectionBuilder()
                .WithUrl(new Uri(new Uri(_baseUrl), "/chatHub"), options =>
                {
                    options.AccessTokenProvider = getToken;

                    options.HttpMessageHandlerFactory = (messageHandler) =>
                    {
                        if (messageHandler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                            return clientHandler;
                        }

                        return new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                        };
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            return conn;
        }
    }
}
