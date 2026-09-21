using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InternTrack.Core.Models;

public class Intern
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string Surname { get; set; } = "";

    public required string Email { get; set; }

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public int UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    [NotMapped]
    public string? Avatar { get; set; }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public List<TaskItem> Tasks { get; set; } = new();
}