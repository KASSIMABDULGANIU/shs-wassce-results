using System.Windows.Controls;
using SHSWassceResultsMgt.Data;

namespace SHSWassceResultsMgt.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += (_, _) => Load();
    }

    private void Load()
    {
        using var db = new AppDbContext();
        CountStudents.Text = db.Students.Count().ToString();
        CountSubjects.Text = db.Subjects.Count().ToString();
        CountScores.Text   = db.Scores.Count().ToString();
        CountStaff.Text    = db.Staff.Count().ToString();
        SubTitle.Text      = $"Welcome — {DateTime.Now:dddd, dd MMMM yyyy}";
    }
}
