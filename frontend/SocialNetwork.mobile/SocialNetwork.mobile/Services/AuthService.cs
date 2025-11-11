using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialNetwork.mobile.DTO;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SocialNetwork.mobile.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _client;
        private const string TokenKey = "auth_token";

        public AuthService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _client = new HttpClient(handler) { BaseAddress = new Uri("https://10.0.2.2:7142") };
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
                Debug.WriteLine($"AuthService.LoginAsync failed: {ex}");
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
                Debug.WriteLine($"AuthService.RegisterAsync failed: {ex}");
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
    }
}
