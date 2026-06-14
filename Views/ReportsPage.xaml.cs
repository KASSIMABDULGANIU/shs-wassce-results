using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;
using SHSWassceResultsMgt.Services;

namespace SHSWassceResultsMgt.Views;

public class ReportRow
{
    public string SubjectName { get; set; } = string.Empty;
    public double ClassScore { get; set; }
    public double ExamScore { get; set; }
    public double Total { get; set; }
    public int Grade { get; set; }
    public string Remark { get; set; } = string.Empty;
}

public partial class ReportsPage : Page
{
    private readonly ObservableCollection<ReportRow> _rows = new();
    private List<Score> _currentScores = new();
    private Student? _currentStudent;

    public ReportsPage()
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

    private void ClassFilter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (ClassFilter.SelectedItem is not string className) return;

        using var db = new AppDbContext();
        StudentFilter.ItemsSource = db.Students
            .Where(s => s.ClassName == className)
            .OrderBy(s => s.Name)
            .ToList();
    }

    private void Preview_Click(object sender, RoutedEventArgs e)
    {
        if (StudentFilter.SelectedItem is not Student student)
        {
            MessageBox.Show("Select a student.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var year = YearBox.Text.Trim();
        var term = TermBox.Text.Trim();

        using var db = new AppDbContext();
        var scores = db.Scores
            .Where(s => s.StudentId == student.Id && s.AcademicYear == year && s.Term == term)
            .OrderBy(s => s.Subject.SortOrder)
            .Select(s => new { s.Id, s.SubjectId, s.ClassScore, s.ExamScore, s.Total, s.Grade, s.Remark, s.AcademicYear, s.Term, SubjectName = s.Subject.Name, SubjectSort = s.Subject.SortOrder })
            .ToList();

        _rows.Clear();
        foreach (var s in scores)
        {
            _rows.Add(new ReportRow
            {
                SubjectName = s.SubjectName,
                ClassScore = s.ClassScore,
                ExamScore = s.ExamScore,
                Total = s.Total,
                Grade = s.Grade,
                Remark = s.Remark
            });
        }

        _currentStudent = student;

        // Reload full Score objects (with navigation property) for PDF generation
        _currentScores = db.Scores
            .Where(s => s.StudentId == student.Id && s.AcademicYear == year && s.Term == term)
            .ToList();

        // Need Subject navigation property loaded for PDF
        foreach (var sc in _currentScores)
            sc.Subject = db.Subjects.First(sub => sub.Id == sc.SubjectId);

        Status.Text = _rows.Count == 0
            ? "No scores found for this student/term/year."
            : $"Showing {_rows.Count} subject(s) for {student.Name}.";
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStudent == null || _currentScores.Count == 0)
        {
            MessageBox.Show("Click 'Preview' first to load a student's results.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF Document (*.pdf)|*.pdf",
            FileName = $"{_currentStudent.Name.Replace(' ', '_')}_ResultSlip_{YearBox.Text.Replace('/', '-')}_T{TermBox.Text}.pdf"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            PdfReportService.GenerateResultSlip(
                _currentStudent, _currentScores, dialog.FileName,
                academicYear: YearBox.Text.Trim(), term: TermBox.Text.Trim());

            Status.Text = $"PDF saved to {dialog.FileName}";

            if (MessageBox.Show("PDF generated successfully. Open it now?", "Success",
                MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to generate PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
