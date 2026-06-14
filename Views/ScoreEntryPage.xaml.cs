using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;
using SHSWassceResultsMgt.Services;

namespace SHSWassceResultsMgt.Views;

/// <summary>
/// Row shown in the score-entry grid. Recalculates Total/Grade/Remark
/// automatically whenever ClassScore or ExamScore is edited.
/// </summary>
public class ScoreRow : INotifyPropertyChanged
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    private double _classScore;
    public double ClassScore
    {
        get => _classScore;
        set { _classScore = value; Recalculate(); }
    }

    private double _examScore;
    public double ExamScore
    {
        get => _examScore;
        set { _examScore = value; Recalculate(); }
    }

    public double Total { get; private set; }
    public int Grade { get; private set; }
    public string Remark { get; private set; } = string.Empty;

    private void Recalculate()
    {
        Total = Math.Round(ClassScore + ExamScore, 2);
        (Grade, Remark) = GradingService.Compute(ClassScore, ExamScore);

        OnChanged(nameof(ClassScore));
        OnChanged(nameof(ExamScore));
        OnChanged(nameof(Total));
        OnChanged(nameof(Grade));
        OnChanged(nameof(Remark));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class ScoreEntryPage : Page
{
    private readonly ObservableCollection<ScoreRow> _rows = new();

    public ScoreEntryPage()
    {
        InitializeComponent();
        ScoresGrid.ItemsSource = _rows;
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

        SubjectFilter.ItemsSource = db.Subjects
            .OrderBy(s => s.SortOrder)
            .ToList();
    }

    private void LoadStudents_Click(object sender, RoutedEventArgs e)
    {
        if (ClassFilter.SelectedItem is not string className ||
            SubjectFilter.SelectedItem is not Subject subject)
        {
            MessageBox.Show("Select a class and subject first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var year = YearBox.Text.Trim();
        var term = TermBox.Text.Trim();

        using var db = new AppDbContext();

        var students = db.Students
            .Where(s => s.ClassName == className)
            .OrderBy(s => s.Name)
            .ToList();

        _rows.Clear();

        foreach (var student in students)
        {
            var existing = db.Scores.FirstOrDefault(sc =>
                sc.StudentId == student.Id &&
                sc.SubjectId == subject.Id &&
                sc.AcademicYear == year &&
                sc.Term == term);

            var row = new ScoreRow
            {
                StudentId = student.Id,
                StudentName = student.Name,
                ClassScore = existing?.ClassScore ?? 0,
                ExamScore = existing?.ExamScore ?? 0
            };

            _rows.Add(row);
        }

        Status.Text = $"Loaded {_rows.Count} student(s) — {subject.Name}, {className} ({year}, Term {term}).";
    }

    private void SaveAll_Click(object sender, RoutedEventArgs e)
    {
        if (SubjectFilter.SelectedItem is not Subject subject)
        {
            MessageBox.Show("Select a subject first.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_rows.Count == 0)
        {
            MessageBox.Show("Load students before saving.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var year = YearBox.Text.Trim();
        var term = TermBox.Text.Trim();

        using var db = new AppDbContext();

        foreach (var row in _rows)
        {
            var existing = db.Scores.FirstOrDefault(sc =>
                sc.StudentId == row.StudentId &&
                sc.SubjectId == subject.Id &&
                sc.AcademicYear == year &&
                sc.Term == term);

            if (existing == null)
            {
                existing = new Score
                {
                    StudentId = row.StudentId,
                    SubjectId = subject.Id,
                    AcademicYear = year,
                    Term = term
                };
                db.Scores.Add(existing);
            }

            existing.ClassScore = row.ClassScore;
            existing.ExamScore = row.ExamScore;
            existing.Total = row.Total;
            existing.Grade = row.Grade;
            existing.Remark = row.Remark;
        }

        db.SaveChanges();
        Status.Text = $"Saved {_rows.Count} score(s) successfully.";
        MessageBox.Show("Scores saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
