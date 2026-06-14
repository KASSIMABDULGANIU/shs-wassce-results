using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;

namespace SHSWassceResultsMgt.Views;

public class RankingRow
{
    public int Position { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int SubjectCount { get; set; }
    public double TotalScore { get; set; }
    public double Average { get; set; }

    // Sum of best 6 grades (WASSCE-style aggregate; lower = better)
    public int Aggregate { get; set; }
}

public partial class RankingsPage : Page
{
    private readonly ObservableCollection<RankingRow> _rows = new();

    public RankingsPage()
    {
        InitializeComponent();
        Grid.ItemsSource = _rows;
        Loaded += (_, _) => Initialize();
    }

    private void Initialize()
    {
        using var db = new AppDbContext();
        ClassFilter.ItemsSource = db.Students
            .Select(s => s.ClassName)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private void Compute_Click(object sender, RoutedEventArgs e)
    {
        if (ClassFilter.SelectedItem is not string className)
        {
            MessageBox.Show("Select a class.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var year = YearBox.Text.Trim();
        var term = TermBox.Text.Trim();

        using var db = new AppDbContext();

        var students = db.Students
            .Where(s => s.ClassName == className)
            .ToList();

        var results = new List<RankingRow>();

        foreach (var student in students)
        {
            var scores = db.Scores
                .Where(s => s.StudentId == student.Id && s.AcademicYear == year && s.Term == term)
                .ToList();

            if (scores.Count == 0)
                continue;

            var totalScore = scores.Sum(s => s.Total);
            var average = Math.Round(totalScore / scores.Count, 2);

            // Best 6 = 6 lowest grades (1 is best in WASSCE numbering)
            var aggregate = scores.OrderBy(s => s.Grade).Take(6).Sum(s => s.Grade);

            results.Add(new RankingRow
            {
                StudentName = student.Name,
                ClassName = student.ClassName,
                SubjectCount = scores.Count,
                TotalScore = Math.Round(totalScore, 2),
                Average = average,
                Aggregate = aggregate
            });
        }

        // Rank by total score descending (highest score = position 1)
        var ranked = results
            .OrderByDescending(r => r.TotalScore)
            .ToList();

        _rows.Clear();
        for (int i = 0; i < ranked.Count; i++)
        {
            ranked[i].Position = i + 1;
            _rows.Add(ranked[i]);
        }

        Status.Text = ranked.Count == 0
            ? "No scores found for this class/term/year combination."
            : $"Ranked {ranked.Count} student(s) for {className} ({year}, Term {term}).";
    }
}
