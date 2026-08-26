using System.ComponentModel.DataAnnotations;

namespace InternTrack.Core.DTOs;

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Departman adı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Departman adı 2 ile 100 karakter arasında olmalıdır.")]
    public required string Name { get; set; }
}
