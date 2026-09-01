using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class CreateInternDto
{
    [Required(ErrorMessage = "İsim alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim 2 ile 100 karakter arasında olmalıdır.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public required string Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir departman seçiniz.")]
    public int DepartmentId { get; set; }
}