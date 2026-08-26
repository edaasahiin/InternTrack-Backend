using System.Text.Json.Serialization;

namespace InternTrack.Core.Models;

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }

    [JsonIgnore]
    public List<Intern> Interns { get; set; } = new();
}
