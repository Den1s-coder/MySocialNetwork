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
using System.Runtime.CompilerServices;

namespace SocialNetwork.mobile.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _client;
        private const string AccessToken = "auth_token";
        private const string RefreshToken = "refresh_token";

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

                LoginResponce loginResponce = null;
                try
                {
                    loginResponce = JsonConvert.DeserializeObject<LoginResponce>(respStr);
                }
                catch
                {
                    try
                    {
                        var obj = JObject.Parse(respStr);
                        var access = obj["AccessToken"];
                        var refresh = obj["RefreshToken"];
                        loginResponce = new LoginResponce
                        {
                            AccessToken = access?.ToString(),
                            RefreshToken = refresh?.ToString(),
                        };
                    }
                    catch
                    {
                        var token = respStr?.Trim('"');
                        loginResponce = new LoginResponce 
                        { 
                            AccessToken = token, 
                            RefreshToken = null 
                        };
                    }
                }

                if (!string.IsNullOrWhiteSpace(loginResponce?.AccessToken))
                {
                    await SecureStorage.SetAsync(AccessToken, loginResponce.AccessToken);
                    if (!string.IsNullOrWhiteSpace(loginResponce.RefreshToken))
                        await SecureStorage.SetAsync(RefreshToken, loginResponce.RefreshToken);

                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponce.AccessToken);
                }

                return loginResponce?.AccessToken;
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
                await SecureStorage.SetAsync(AccessToken, string.Empty);
                await SecureStorage.SetAsync(RefreshToken, string.Empty);
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
                return await SecureStorage.GetAsync(AccessToken);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetRefreshTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync(RefreshToken);
            }
            catch
            {
                return null;
            }
        }
    }
}
