using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token zorunludur.")]
    public required string RefreshToken { get; set; }
}