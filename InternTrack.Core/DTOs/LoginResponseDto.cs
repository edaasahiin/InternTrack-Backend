namespace InternTrack.Core.DTOs;

public class LoginResponseDto
{
    public required string Token { get; set; }

    public required string RefreshToken { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string Role { get; set; }
}