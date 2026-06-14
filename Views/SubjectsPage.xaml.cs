using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Views;

public partial class SubjectsPage : Page
{
    private readonly ObservableCollection<Subject> _list = new();
    private Subject? _selected;

    public SubjectsPage()
    {
        InitializeComponent();
        Grid.ItemsSource = _list;
        Loaded += (_, _) => Reload();
    }

    private void Reload()
    {
        using var db = new AppDbContext();
        _list.Clear();
        foreach (var s in db.Subjects.OrderBy(s => s.Program).ThenBy(s => s.SortOrder))
            _list.Add(s);
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = Grid.SelectedItem as Subject;
        if (_selected is null) return;
        SubjectName.Text = _selected.Name;
        ProgramBox.Text  = _selected.Program;
        IsCoreBox.IsChecked = _selected.IsCore;
        SortBox.Text     = _selected.SortOrder.ToString();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SubjectName.Text))
        {
            MessageBox.Show("Subject name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var subject = new Subject
        {
            Name = SubjectName.Text.Trim(),
            Program = string.IsNullOrWhiteSpace(ProgramBox.Text) ? "Core" : ProgramBox.Text.Trim(),
            IsCore = IsCoreBox.IsChecked == true,
            SortOrder = int.TryParse(SortBox.Text, out var n) ? n : 0
        };

        using var db = new AppDbContext();
        db.Subjects.Add(subject);
        db.SaveChanges();

        Reload();
        Clear_Click(sender, e);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            MessageBox.Show("Select a subject to delete.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Delete '{_selected.Name}'? Any scores recorded for this subject will also be removed.",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        using var db = new AppDbContext();
        db.Scores.RemoveRange(db.Scores.Where(sc => sc.SubjectId == _selected.Id));
        db.Subjects.Remove(db.Subjects.Find(_selected.Id)!);
        db.SaveChanges();

        Reload();
        Clear_Click(sender, e);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        SubjectName.Text = string.Empty;
        ProgramBox.Text = string.Empty;
        IsCoreBox.IsChecked = false;
        SortBox.Text = string.Empty;
        _selected = null;
        Grid.SelectedItem = null;
    }
}
