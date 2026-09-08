using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Mevcut şifre alanı zorunludur.")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
    [MinLength(
        6,
        ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır."
    )]
    public required string NewPassword { get; set; }
}