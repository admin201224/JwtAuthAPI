namespace JwtAuthAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }

        // OAuth fields
        public string? GoogleId { get; set; }
        public string? GitHubId { get; set; }
        public string? MicrosoftId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
