namespace InternTrack.Core.Models;

public class RefreshToken
{
    public int Id { get; set; }

    public required string Token { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }
}