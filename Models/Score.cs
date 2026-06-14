namespace SHSWassceResultsMgt.Models;

public class Score
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    // Class Assessment (out of 50 for WASSCE)
    public double ClassScore { get; set; }

    // End of Term / WASSCE Exam (out of 50)
    public double ExamScore { get; set; }

    // Total out of 100 (stored for fast querying/ranking)
    public double Total { get; set; }

    // WASSCE numeric grade 1-9
    public int Grade { get; set; }

    // e.g. Excellent, Very Good, Credit, Pass, Fail
    public string Remark { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public string Term { get; set; } = string.Empty;
}
