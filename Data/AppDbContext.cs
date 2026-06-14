using System.IO;
using Microsoft.EntityFrameworkCore;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Data;

public class AppDbContext : DbContext
{
    // DB file sits next to the .exe — fully portable, no server needed
    private static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "shs_wassce.db");

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Score> Scores { get; set; } = null!;
    public DbSet<GradingScale> GradingScales { get; set; } = null!;
    public DbSet<Staff> Staff { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Prevent duplicate scores per student/subject/year/term
        modelBuilder.Entity<Score>()
            .HasIndex(s => new { s.StudentId, s.SubjectId, s.AcademicYear, s.Term })
            .IsUnique();

        // Seed WASSCE grading scale (Ghana standard)
        modelBuilder.Entity<GradingScale>().HasData(
            new GradingScale { Id = 1, MinScore = 80, MaxScore = 100,   Grade = 1, Remark = "Excellent" },
            new GradingScale { Id = 2, MinScore = 75, MaxScore = 79.99, Grade = 2, Remark = "Very Good" },
            new GradingScale { Id = 3, MinScore = 70, MaxScore = 74.99, Grade = 3, Remark = "Good" },
            new GradingScale { Id = 4, MinScore = 65, MaxScore = 69.99, Grade = 4, Remark = "Credit" },
            new GradingScale { Id = 5, MinScore = 60, MaxScore = 64.99, Grade = 5, Remark = "Credit" },
            new GradingScale { Id = 6, MinScore = 55, MaxScore = 59.99, Grade = 6, Remark = "Credit" },
            new GradingScale { Id = 7, MinScore = 50, MaxScore = 54.99, Grade = 7, Remark = "Pass" },
            new GradingScale { Id = 8, MinScore = 40, MaxScore = 49.99, Grade = 8, Remark = "Pass" },
            new GradingScale { Id = 9, MinScore = 0,  MaxScore = 39.99, Grade = 9, Remark = "Fail" }
        );

        // Seed core subjects (taken by all programs)
        modelBuilder.Entity<Subject>().HasData(
            new Subject { Id = 1,  Name = "English Language",         Program = "Core", IsCore = true,  SortOrder = 1 },
            new Subject { Id = 2,  Name = "Core Mathematics",         Program = "Core", IsCore = true,  SortOrder = 2 },
            new Subject { Id = 3,  Name = "Integrated Science",       Program = "Core", IsCore = true,  SortOrder = 3 },
            new Subject { Id = 4,  Name = "Social Studies",           Program = "Core", IsCore = true,  SortOrder = 4 },

            // General Arts electives
            new Subject { Id = 5,  Name = "Literature in English",    Program = "General Arts", IsCore = false, SortOrder = 5 },
            new Subject { Id = 6,  Name = "Government",               Program = "General Arts", IsCore = false, SortOrder = 6 },
            new Subject { Id = 7,  Name = "History",                  Program = "General Arts", IsCore = false, SortOrder = 7 },
            new Subject { Id = 8,  Name = "Economics",                Program = "General Arts", IsCore = false, SortOrder = 8 },
            new Subject { Id = 9,  Name = "Geography",                Program = "General Arts", IsCore = false, SortOrder = 9 },
            new Subject { Id = 10, Name = "French",                   Program = "General Arts", IsCore = false, SortOrder = 10 },

            // Business electives
            new Subject { Id = 11, Name = "Business Management",      Program = "Business", IsCore = false, SortOrder = 5 },
            new Subject { Id = 12, Name = "Accounting",               Program = "Business", IsCore = false, SortOrder = 6 },
            new Subject { Id = 13, Name = "Economics",                Program = "Business", IsCore = false, SortOrder = 7 },
            new Subject { Id = 14, Name = "Elective Mathematics",     Program = "Business", IsCore = false, SortOrder = 8 },

            // Science electives
            new Subject { Id = 15, Name = "Elective Mathematics",     Program = "Science", IsCore = false, SortOrder = 5 },
            new Subject { Id = 16, Name = "Physics",                  Program = "Science", IsCore = false, SortOrder = 6 },
            new Subject { Id = 17, Name = "Chemistry",                Program = "Science", IsCore = false, SortOrder = 7 },
            new Subject { Id = 18, Name = "Biology",                  Program = "Science", IsCore = false, SortOrder = 8 }
        );

        try
        {
            var types = string.Join("\n", modelBuilder.Model.GetEntityTypes().Select(t => t.Name));
            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modeltypes.log"), types + "\n\n");
        }
        catch
        {
            // ignore logging failures
        }
    }
}
