using InternTrack.Core.Constants;
using System.Text.Json.Serialization;

namespace InternTrack.Core.Models;

public class User
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string Surname { get; set; } = "";

    public string? Avatar { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public string Role { get; set; } = Roles.Intern;

    public bool MustChangePassword { get; set; } = false;

    [JsonIgnore]
    public Intern? Intern { get; set; }

    [JsonIgnore]
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
