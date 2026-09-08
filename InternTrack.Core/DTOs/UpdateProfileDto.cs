using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "İsim alanı zorunludur.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "İsim 2 ile 100 karakter arasında olmalıdır."
    )]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Soyad 2 ile 100 karakter arasında olmalıdır."
    )]
    public required string Surname { get; set; }

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public required string Email { get; set; }
}