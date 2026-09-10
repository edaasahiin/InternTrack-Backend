using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class UpdateTaskDto
{
    [Required(ErrorMessage = "Görev başlığı zorunludur.")]
    [StringLength(
        150,
        MinimumLength = 2,
        ErrorMessage = "Görev başlığı 2 ile 150 karakter arasında olmalıdır."
    )]
    public required string Title { get; set; }

    [StringLength(
        500,
        ErrorMessage = "Açıklama en fazla 500 karakter olabilir."
    )]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Görev durumu zorunludur.")]
    public required string Status { get; set; }

    [Required(ErrorMessage = "Görev önceliği zorunludur.")]
    public string Priority { get; set; } = "Medium";

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Geçerli bir stajyer seçiniz."
    )]
    public int InternId { get; set; }

    public bool CanInternDeleteWhenCompleted { get; set; }
}