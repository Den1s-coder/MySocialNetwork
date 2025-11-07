using SocialNetwork.mobile.Models;
using SocialNetwork.mobile.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using System.Linq;
using System.Diagnostics;
using SocialNetwork.mobile.DTO;

namespace SocialNetwork.mobile.Services
{
    public class ApiDataStore : IDataStore<Item>, IAuthService
    {
        private readonly HttpClient _client;
        private const string TokenKey = "auth_token";

        public ApiDataStore()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _client = new HttpClient(handler) { BaseAddress = new Uri("https://10.0.2.2:7142") };
        }

        private async Task SetAuthHeader()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            var posts = await GetPostsAsync();
            return posts.Select(p => new Item { Id = p.Id, Text = p.Text, Description = p.UserName });
        }

        public async Task<Item> GetItemAsync(string id)
        {
            await SetAuthHeader();
            try
            {
                var resp = await _client.GetAsync($"/api/post/{id}");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                var p = JsonConvert.DeserializeObject<PostDto>(json);
                return new Item { Id = p.Id.ToString(), Text = p.Text, Description = p.UserName };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            await SetAuthHeader();
            var dto = new CreatePostDto { Text = item.Text };
            var json = JsonConvert.SerializeObject(dto);
            try
            {
                var resp = await _client.PostAsync("/api/post", new StringContent(json, Encoding.UTF8, "application/json"));
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            await SetAuthHeader();
            return await Task.FromResult(false);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            await SetAuthHeader();
            try
            {
                var resp = await _client.DeleteAsync($"/api/post/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Post>> GetPostsAsync(bool forceRefresh = false)
        {
            await SetAuthHeader();
            var url = "/api/post"; 
            try
            {
                var resp = await _client.GetAsync(url);

                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    return new List<Post>();
                }

                var json = await resp.Content.ReadAsStringAsync();
                var posts = JsonConvert.DeserializeObject<List<PostDto>>(json);
                var result = new List<Post>();
                if (posts != null)
                {
                    foreach (var p in posts)
                    {
                        result.Add(MapPostDto(p));
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new List<Post>();
            }
        }

        public async Task<Post> GetPostAsync(string id)
        {
            await SetAuthHeader();
            try
            {
                var resp = await _client.GetAsync($"/api/post/{id}");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                var p = JsonConvert.DeserializeObject<PostDto>(json);
                return MapPostDto(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetPostAsync failed: {ex}");
                return null;
            }
        }

        public async Task<bool> CreateCommentAsync(string postId, string text)
        {
            await SetAuthHeader();
            var dto = new { Text = text, PostId = Guid.Parse(postId) };
            var json = JsonConvert.SerializeObject(dto);
            try
            {
                var resp = await _client.PostAsync("/api/comment/CreateComment", new StringContent(json, Encoding.UTF8, "application/json"));
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateCommentAsync failed: {ex}");
                return false;
            }
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var dto = new { Username = username, Password = password };
            var json = JsonConvert.SerializeObject(dto);
            try
            {
                var resp = await _client.PostAsync("/api/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
                resp.EnsureSuccessStatusCode();
                var respStr = await resp.Content.ReadAsStringAsync();

                string token = null;
                try
                {
                    token = JsonConvert.DeserializeObject<string>(respStr);
                }
                catch
                {
                    try
                    {
                        var obj = JObject.Parse(respStr);
                        if (obj["token"] != null)
                            token = obj["token"].ToString();
                    }
                    catch
                    {
                        token = respStr?.Trim('"');
                    }
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await SecureStorage.SetAsync(TokenKey, token);
                }

                return token;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoginAsync failed: {ex}");
                throw;
            }
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            var dto = new { UserName = username, Email = email, Password = password };
            var json = JsonConvert.SerializeObject(dto);
            try
            {
                var resp = await _client.PostAsync("/api/auth/register", new StringContent(json, Encoding.UTF8, "application/json"));
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RegisterAsync failed: {ex}");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await SecureStorage.SetAsync(TokenKey, string.Empty);
            }
            catch
            {
            }
            _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(TokenKey);
            }
            catch
            {
                return null;
            }
        }

        private Post MapPostDto(PostDto p)
        {
            if (p == null) return null;
            var post = new Post
            {
                Id = p.Id.ToString(),
                Text = p.Text,
                ImageUrl = p.ImageUrl,
                UserName = p.UserName,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsBanned = p.IsBanned,
                Comments = p.Comments?.Select(c => new Comment
                {
                    Id = c.Id.ToString(),
                    Text = c.Text,
                    UserName = c.UserName,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsBanned = c.IsBanned
                }).ToList() ?? new List<Comment>()
            };
            return post;
        }
    }
}
