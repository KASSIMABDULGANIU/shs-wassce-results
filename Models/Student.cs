using System.ComponentModel.DataAnnotations;

namespace SHSWassceResultsMgt.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string IndexNumber { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    // e.g. General Arts, Business, Science, Visual Arts, Home Economics
    public string Program { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public string Term { get; set; } = string.Empty;

    public ICollection<Score> Scores { get; set; } = new List<Score>();
}
