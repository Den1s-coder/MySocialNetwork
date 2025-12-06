using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialNetwork.mobile.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SocialNetwork.mobile.ViewModels
{
    public class ProfileViewModel: BaseViewModel
    {
        private const string TokenKey = "auth_token";
        private readonly HttpClient _client;

        private string id;
        private string userName;
        private string email;
        private string bio;
        private string avatarUrl;
        private bool isEditing;

        public Command LoadProfileCommand { get; }
        public Command SaveProfileCommand { get; }
        public Command EditCommand { get; }
        public Command LogoutCommand { get; }
        public Command UploadAvatarCommand { get; }

        public string Id { get => id; set => SetProperty(ref id, value); }
        public string UserName { get => userName; set => SetProperty(ref userName, value); }
        public string Email { get => email; set => SetProperty(ref email, value); }
        public string Bio { get => bio; set => SetProperty(ref bio, value); }
        public string AvatarUrl { get => avatarUrl; set => SetProperty(ref avatarUrl, value); }
        public bool IsEditing { get => isEditing; set => SetProperty(ref isEditing, value); }

        public ProfileViewModel()
        {
            Title = "Profile";
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _client = new HttpClient(handler) { BaseAddress = new Uri("https://10.0.2.2:7142") };

            LoadProfileCommand = new Command(async () => await LoadProfileAsync());
            SaveProfileCommand = new Command(async () => await SaveProfileAsync());
            EditCommand = new Command(() => IsEditing = !IsEditing);
            LogoutCommand = new Command(async () => await LogoutAsync());
            UploadAvatarCommand = new Command(async () => await UploadAvatarAsync());
        }

        private async Task SetAuthHeaderAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task LoadProfileAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await SetAuthHeaderAsync();
                var resp = await _client.GetAsync("/api/user/profile");
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine($"LoadProfileAsync non-success: {resp.StatusCode} body: {err}");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load profile", "OK");
                    return;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<UserProfileDto>(json);
                if (dto != null)
                {
                    Id = dto.Id.ToString();
                    UserName = dto.Name;
                    Email = dto.Email;
                    Bio = dto.Bio;
                    AvatarUrl = dto.ProfilePictureUrl;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadProfileAsync failed: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> TryLoadProfileAsync()
        {
            if (IsBusy) return false;
            IsBusy = true;
            try
            {
                await SetAuthHeaderAsync();
                var resp = await _client.GetAsync("/api/user/profile");
                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"TryLoadProfileAsync non-success: {resp.StatusCode}");
                    return false;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<UserProfileDto>(json);
                if (dto == null) return false;

                Id = dto.Id.ToString();
                UserName = dto.Name;
                Email = dto.Email;
                Bio = dto.Bio;
                AvatarUrl = dto.ProfilePictureUrl;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryLoadProfileAsync failed: {ex}");
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SaveProfileAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await SetAuthHeaderAsync();
                var dto = new { Name = UserName, Email = Email, Bio = Bio, ProfilePictureUrl = AvatarUrl };
                var json = JsonConvert.SerializeObject(dto);

                var resp = await _client.PutAsync("/api/user/profile", new StringContent(json, Encoding.UTF8, "application/json"));
                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine($"SaveProfileAsync non-success: {resp.StatusCode} body: {err}");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to save profile", "OK");
                    return;
                }

                IsEditing = false;
                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveProfileAsync failed: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LogoutAsync()
        {
            try
            {
                await SecureStorage.SetAsync(TokenKey, string.Empty);
            }
            catch { }
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public async Task UploadAvatarAsync()
        {
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Select avatar" });
                if (result == null) return;

                using (var stream = await result.OpenReadAsync())
                {
                    var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    content.Add(streamContent, "file", result.FileName);

                    await SetAuthHeaderAsync();
                    var resp = await _client.PostAsync("/api/File/upload", content);
                    if (!resp.IsSuccessStatusCode)
                    {
                        var err = await resp.Content.ReadAsStringAsync();
                        Debug.WriteLine($"UploadAvatarAsync failed: {resp.StatusCode} body: {err}");
                        await Application.Current.MainPage.DisplayAlert("Error", "Upload failed", "OK");
                        return;
                    }

                    var json = await resp.Content.ReadAsStringAsync();

                    var j = JObject.Parse(json);
                    string fileUrl = j["fileUrl"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(fileUrl))
                    {
                        AvatarUrl = fileUrl;
                        await SaveProfileAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UploadAvatarAsync exception: {ex}");
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

