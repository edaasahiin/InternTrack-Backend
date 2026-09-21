using InternTrack.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Görev başlığı zorunludur.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Görev başlığı 2 ile 150 karakter arasında olmalıdır.")]
    public required string Title { get; set; }

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Görev durumu zorunludur.")]
    [RegularExpression(TaskStatuses.ValidationPattern, ErrorMessage = "Görev durumu ToDo, InProgress veya Done olmalıdır.")]
    public required string Status { get; set; }

    [Required(ErrorMessage = "Görev önceliği zorunludur.")]
    [RegularExpression(TaskPriorities.ValidationPattern, ErrorMessage = "Görev önceliği Low, Medium veya High olmalıdır.")]
    public string Priority { get; set; } = TaskPriorities.Medium;

    public DateTime? DueDate { get; set; }

    public int InternId { get; set; }

    public bool CanInternDeleteWhenCompleted { get; set; } = false;
}
