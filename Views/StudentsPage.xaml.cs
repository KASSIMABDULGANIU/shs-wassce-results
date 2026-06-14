using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Views;

public partial class StudentsPage : Page
{
    private readonly ObservableCollection<Student> _list = new();
    private Student? _selected;

    public StudentsPage()
    {
        InitializeComponent();
        Grid.ItemsSource = _list;
        Loaded += (_, _) => Reload();
    }

    private void Reload()
    {
        using var db = new AppDbContext();
        _list.Clear();
        foreach (var s in db.Students.OrderBy(s => s.ClassName).ThenBy(s => s.Name))
            _list.Add(s);
        Status.Text = $"{_list.Count} student(s) loaded.";
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = Grid.SelectedItem as Student;
        if (_selected is null) return;
        NameBox.Text    = _selected.Name;
        IndexBox.Text   = _selected.IndexNumber;
        ClassBox.Text   = _selected.ClassName;
        ProgramBox.Text = _selected.Program;
        YearBox.Text    = _selected.AcademicYear;
        TermBox.Text    = _selected.Term;
        foreach (ComboBoxItem item in GenderBox.Items)
            if (item.Content?.ToString() == _selected.Gender)
            { GenderBox.SelectedItem = item; break; }
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (!Validate()) return;
        using var db = new AppDbContext();
        db.Students.Add(Build());
        db.SaveChanges();
        Reload(); ClearForm();
    }

    private void Update_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { Msg("Select a student to update."); return; }
        if (!Validate()) return;
        using var db = new AppDbContext();
        var s = db.Students.Find(_selected.Id)!;
        Apply(s); db.SaveChanges();
        Reload(); ClearForm();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) { Msg("Select a student to delete."); return; }
        if (MessageBox.Show($"Delete {_selected.Name}? Their scores will also be removed.",
            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        using var db = new AppDbContext();
        db.Scores.RemoveRange(db.Scores.Where(sc => sc.StudentId == _selected.Id));
        db.Students.Remove(db.Students.Find(_selected.Id)!);
        db.SaveChanges();
        Reload(); ClearForm();
    }

    private void Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

    // ── Helpers ───────────────────────────────────────────────
    private Student Build() => new()
    {
        Name         = NameBox.Text.Trim(),
        IndexNumber  = IndexBox.Text.Trim(),
        Gender       = (GenderBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
        ClassName    = ClassBox.Text.Trim(),
        Program      = ProgramBox.Text.Trim(),
        AcademicYear = YearBox.Text.Trim(),
        Term         = TermBox.Text.Trim()
    };

    private void Apply(Student s)
    {
        s.Name         = NameBox.Text.Trim();
        s.IndexNumber  = IndexBox.Text.Trim();
        s.Gender       = (GenderBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        s.ClassName    = ClassBox.Text.Trim();
        s.Program      = ProgramBox.Text.Trim();
        s.AcademicYear = YearBox.Text.Trim();
        s.Term         = TermBox.Text.Trim();
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))  { Msg("Name is required.");  return false; }
        if (string.IsNullOrWhiteSpace(ClassBox.Text)) { Msg("Class is required."); return false; }
        return true;
    }

    private void ClearForm()
    {
        NameBox.Text = IndexBox.Text = ClassBox.Text =
        ProgramBox.Text = YearBox.Text = TermBox.Text = "";
        GenderBox.SelectedItem = null;
        _selected = null;
        Grid.SelectedItem = null;
    }

    private static void Msg(string m)
        => MessageBox.Show(m, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
}
