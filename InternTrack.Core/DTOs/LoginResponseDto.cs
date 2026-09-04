namespace InternTrack.Core.DTOs;

public class LoginResponseDto
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }

    public required string Name { get; set; }

    public string Surname { get; set; } = "";

    public string? Avatar { get; set; }

    public required string Email { get; set; }

    public required string Role { get; set; }
}