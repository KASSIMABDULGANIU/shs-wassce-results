namespace SHSWassceResultsMgt.Models;

public class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Which program this subject belongs to — "Core" means all programs
    public string Program { get; set; } = string.Empty;

    public bool IsCore { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Score> Scores { get; set; } = new List<Score>();
}
