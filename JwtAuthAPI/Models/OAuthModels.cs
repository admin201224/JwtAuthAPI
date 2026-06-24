namespace JwtAuthAPI.Models
{
    public class OAuthSettings
    {
        public string GoogleClientId { get; set; } = null!;
        public string GoogleClientSecret { get; set; } = null!;
        public string GoogleRedirectUri { get; set; } = null!;

        public string GitHubClientId { get; set; } = null!;
        public string GitHubClientSecret { get; set; } = null!;
        public string GitHubRedirectUri { get; set; } = null!;

        public string MicrosoftClientId { get; set; } = null!;
        public string MicrosoftClientSecret { get; set; } = null!;
        public string MicrosoftRedirectUri { get; set; } = null!;
    }

    public class OAuthTokenRequest
    {
        public string Code { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Provider { get; set; } = null!; // "google", "github", "microsoft"
    }

    public class OAuthTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Token { get; set; } = null!;
        public int ExpiresIn { get; set; }
    }

    public class OAuthUserInfo
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Picture { get; set; } = null!;
        public string Provider { get; set; } = null!;
    }
}
