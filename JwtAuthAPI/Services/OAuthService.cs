using JwtAuthAPI.Data;
using JwtAuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace JwtAuthAPI.Services
{
    public interface IOAuthService
    {
        Task<OAuthUserInfo?> GetGoogleUserInfoAsync(string accessToken);
        Task<OAuthUserInfo?> GetGitHubUserInfoAsync(string accessToken);
        Task<OAuthUserInfo?> GetMicrosoftUserInfoAsync(string accessToken);
        Task<User?> SignInOrCreateUserAsync(OAuthUserInfo userInfo);
    }

    public class OAuthService : IOAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(HttpClient httpClient, ApplicationDbContext db, ILogger<OAuthService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Get Google user info from access token
        /// </summary>
        public async Task<OAuthUserInfo?> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                var url = $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new OAuthUserInfo
                {
                    Id = root.GetProperty("id").GetString() ?? "",
                    Email = root.GetProperty("email").GetString() ?? "",
                    Name = root.GetProperty("name").GetString() ?? "",
                    Picture = root.TryGetProperty("picture", out var pic) ? pic.GetString() : "",
                    Provider = "google"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Google user info");
                return null;
            }
        }

        /// <summary>
        /// Get GitHub user info from access token
        /// </summary>
        public async Task<OAuthUserInfo?> GetGitHubUserInfoAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                var response = await _httpClient.GetAsync("https://api.github.com/user");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new OAuthUserInfo
                {
                    Id = root.GetProperty("id").GetInt32().ToString(),
                    Email = root.TryGetProperty("email", out var email) ? email.GetString() ?? "" : "",
                    Name = root.GetProperty("name").GetString() ?? root.GetProperty("login").GetString() ?? "",
                    Picture = root.GetProperty("avatar_url").GetString() ?? "",
                    Provider = "github"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GitHub user info");
                return null;
            }
        }

        /// <summary>
        /// Get Microsoft user info from access token
        /// </summary>
        public async Task<OAuthUserInfo?> GetMicrosoftUserInfoAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new OAuthUserInfo
                {
                    Id = root.GetProperty("id").GetString() ?? "",
                    Email = root.TryGetProperty("mail", out var mail) ? mail.GetString() ?? "" : root.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() ?? "" : "",
                    Name = root.GetProperty("displayName").GetString() ?? "",
                    Picture = "", // Microsoft doesn't provide avatar in simple request
                    Provider = "microsoft"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Microsoft user info");
                return null;
            }
        }

        /// <summary>
        /// Sign in or create user from OAuth info
        /// </summary>
        public async Task<User?> SignInOrCreateUserAsync(OAuthUserInfo userInfo)
        {
            try
            {
                // Try to find existing user by OAuth provider ID
                User? user = userInfo.Provider switch
                {
                    "google" => await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == userInfo.Id),
                    "github" => await _db.Users.FirstOrDefaultAsync(u => u.GitHubId == userInfo.Id),
                    "microsoft" => await _db.Users.FirstOrDefaultAsync(u => u.MicrosoftId == userInfo.Id),
                    _ => null
                };

                if (user != null)
                {
                    _logger.LogInformation($"User {user.Username} logged in via {userInfo.Provider}");
                    return user;
                }

                // Try to find by email
                user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userInfo.Email);

                if (user == null)
                {
                    // Create new user
                    user = new User
                    {
                        Username = userInfo.Email?.Split('@')[0] ?? userInfo.Name,
                        Email = userInfo.Email,
                        Avatar = userInfo.Picture,
                        Role = "User",
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Update OAuth ID based on provider
                switch (userInfo.Provider)
                {
                    case "google":
                        user.GoogleId = userInfo.Id;
                        break;
                    case "github":
                        user.GitHubId = userInfo.Id;
                        break;
                    case "microsoft":
                        user.MicrosoftId = userInfo.Id;
                        break;
                }

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"New user created via {userInfo.Provider}: {user.Email}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signing in or creating user");
                return null;
            }
        }
    }
}
