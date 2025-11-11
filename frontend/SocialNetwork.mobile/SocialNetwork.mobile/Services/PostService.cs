using Newtonsoft.Json;
using SocialNetwork.mobile.DTO;
using SocialNetwork.mobile.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SocialNetwork.mobile.Services
{
    public class PostService
    {
        private readonly HttpClient _client;
        private const string TokenKey = "auth_token";

        public PostService()
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
                Debug.WriteLine($"PostService.GetPostsAsync failed: {ex}");
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
                Debug.WriteLine($"PostService.GetPostAsync failed: {ex}");
                return null;
            }
        }

        public async Task<bool> CreatePostAsync(string text)
        {
            await SetAuthHeader();
            var dto = new CreatePostDto { Text = text };
            var json = JsonConvert.SerializeObject(dto);
            try
            {
                var resp = await _client.PostAsync("/api/post", new StringContent(json, Encoding.UTF8, "application/json"));
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PostService.CreatePostAsync failed: {ex}");
                return false;
            }
        }

        public async Task<bool> DeletePostAsync(string id)
        {
            await SetAuthHeader();
            try
            {
                var resp = await _client.DeleteAsync($"/api/post/{id}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PostService.DeletePostAsync failed: {ex}");
                return false;
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
                Debug.WriteLine($"PostService.CreateCommentAsync failed: {ex}");
                return false;
            }
        }

        public async Task<IEnumerable<Post>> GetMyPostsAsync()
        {
            await SetAuthHeader();
            var url = "/api/post/profile";
            try
            {
                Debug.WriteLine($"GetMyPostsAsync calling {_client.BaseAddress}{url}");
                var resp = await _client.GetAsync(url);
                Debug.WriteLine($"GetMyPostsAsync response status: {resp.StatusCode}");
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine($"GetMyPostsAsync returned non-success: {resp.StatusCode}, body: {body}");
                    return new List<Post>();
                }

                var json = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine($"GetMyPostsAsync response body length: {json?.Length}");
                var posts = JsonConvert.DeserializeObject<List<PostDto>>(json);
                var result = new List<Post>();
                if (posts != null)
                {
                    foreach (var p in posts)
                    {
                        result.Add(MapPostDto(p));
                    }
                }
                Debug.WriteLine($"GetMyPostsAsync parsed {result.Count} posts");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PostService.GetMyPostsAsync failed: {ex}");
                return new List<Post>();
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

        private async Task<string> GetTokenAsync()
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
    }
}
