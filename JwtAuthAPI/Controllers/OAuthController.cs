using JwtAuthAPI.Models;
using JwtAuthAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

namespace JwtAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OAuthController> _logger;
        private readonly HttpClient _httpClient;

        public OAuthController(
            IOAuthService oauthService,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<OAuthController> logger,
            HttpClient httpClient)
        {
            _oauthService = oauthService;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Google OAuth Callback
        /// Exchange authorization code for tokens
        /// </summary>
        [HttpPost("google")]
        public async Task<IActionResult> GoogleAuth([FromBody] OAuthTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                return BadRequest("Authorization code is required");

            try
            {
                var clientId = _configuration["OAuth:Google:ClientId"];
                var clientSecret = _configuration["OAuth:Google:ClientSecret"];
                var redirectUri = _configuration["OAuth:Google:RedirectUri"];

                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "code", request.Code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Failed to get access token from Google");

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Get user info
                var userInfo = await _oauthService.GetGoogleUserInfoAsync(accessToken);
                if (userInfo == null)
                    return BadRequest("Failed to get user info from Google");

                // Sign in or create user
                var user = await _oauthService.SignInOrCreateUserAsync(userInfo);
                if (user == null)
                    return BadRequest("Failed to create or retrieve user");

                // Generate JWT tokens
                var jwtToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _logger.LogInformation($"User {user.Email} authenticated via Google");

                return Ok(new
                {
                    message = "Google authentication successful",
                    accessToken = jwtToken,
                    refreshTokenId = refreshToken.TokenId,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        name = user.Username,
                        avatar = user.Avatar,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google authentication error");
                return StatusCode(500, "Google authentication failed");
            }
        }

        /// <summary>
        /// GitHub OAuth Callback
        /// </summary>
        [HttpPost("github")]
        public async Task<IActionResult> GitHubAuth([FromBody] OAuthTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                return BadRequest("Authorization code is required");

            try
            {
                var clientId = _configuration["OAuth:GitHub:ClientId"];
                var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];

                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "code", request.Code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret }
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.PostAsync("https://github.com/login/oauth/access_token", content);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Failed to get access token from GitHub");

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Get user info
                var userInfo = await _oauthService.GetGitHubUserInfoAsync(accessToken);
                if (userInfo == null)
                    return BadRequest("Failed to get user info from GitHub");

                // Sign in or create user
                var user = await _oauthService.SignInOrCreateUserAsync(userInfo);
                if (user == null)
                    return BadRequest("Failed to create or retrieve user");

                // Generate JWT tokens
                var jwtToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _logger.LogInformation($"User {user.Email} authenticated via GitHub");

                return Ok(new
                {
                    message = "GitHub authentication successful",
                    accessToken = jwtToken,
                    refreshTokenId = refreshToken.TokenId,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        avatar = user.Avatar,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GitHub authentication error");
                return StatusCode(500, "GitHub authentication failed");
            }
        }

        /// <summary>
        /// Microsoft OAuth Callback
        /// </summary>
        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftAuth([FromBody] OAuthTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
                return BadRequest("Authorization code is required");

            try
            {
                var clientId = _configuration["OAuth:Microsoft:ClientId"];
                var clientSecret = _configuration["OAuth:Microsoft:ClientSecret"];
                var redirectUri = _configuration["OAuth:Microsoft:RedirectUri"];

                // Exchange code for access token
                var tokenRequest = new Dictionary<string, string>
                {
                    { "code", request.Code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);

                if (!response.IsSuccessStatusCode)
                    return BadRequest("Failed to get access token from Microsoft");

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();

                // Get user info
                var userInfo = await _oauthService.GetMicrosoftUserInfoAsync(accessToken);
                if (userInfo == null)
                    return BadRequest("Failed to get user info from Microsoft");

                // Sign in or create user
                var user = await _oauthService.SignInOrCreateUserAsync(userInfo);
                if (user == null)
                    return BadRequest("Failed to create or retrieve user");

                // Generate JWT tokens
                var jwtToken = _tokenService.GenerateAccessToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _logger.LogInformation($"User {user.Email} authenticated via Microsoft");

                return Ok(new
                {
                    message = "Microsoft authentication successful",
                    accessToken = jwtToken,
                    refreshTokenId = refreshToken.TokenId,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        username = user.Username,
                        avatar = user.Avatar,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Microsoft authentication error");
                return StatusCode(500, "Microsoft authentication failed");
            }
        }

        /// <summary>
        /// Get OAuth redirect URLs
        /// </summary>
        [HttpGet("urls")]
        public IActionResult GetOAuthUrls()
        {
            var googleClientId = _configuration["OAuth:Google:ClientId"];
            var googleRedirectUri = _configuration["OAuth:Google:RedirectUri"];
            var githubClientId = _configuration["OAuth:GitHub:ClientId"];
            var githubRedirectUri = _configuration["OAuth:GitHub:RedirectUri"];
            var microsoftClientId = _configuration["OAuth:Microsoft:ClientId"];

            return Ok(new
            {
                google = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={googleClientId}&redirect_uri={Uri.EscapeDataString(googleRedirectUri)}&response_type=code&scope=openid%20email%20profile",
                github = $"https://github.com/login/oauth/authorize?client_id={githubClientId}&redirect_uri={Uri.EscapeDataString(githubRedirectUri)}&scope=user:email",
                microsoft = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={microsoftClientId}&response_type=code&scope=openid%20profile%20email&redirect_uri={Uri.EscapeDataString(_configuration["OAuth:Microsoft:RedirectUri"])}"
            });
        }
    }
}
