using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;
using SHSWassceResultsMgt.Services;

namespace SHSWassceResultsMgt.Views;

public partial class SettingsPage : Page
{
    private readonly ObservableCollection<GradingScale> _scales = new();

    public SettingsPage()
    {
        InitializeComponent();
        Grid.ItemsSource = _scales;
        Loaded += (_, _) => Reload();
    }

    private void Reload_Click(object sender, RoutedEventArgs e) => Reload();

    private void Reload()
    {
        using var db = new AppDbContext();
        _scales.Clear();
        foreach (var s in db.GradingScales.OrderByDescending(g => g.MinScore))
            _scales.Add(s);
        Status.Text = "Loaded current grading scale.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        using var db = new AppDbContext();

        foreach (var scale in _scales)
        {
            var entity = db.GradingScales.Find(scale.Id);
            if (entity == null) continue;

            entity.MinScore = scale.MinScore;
            entity.MaxScore = scale.MaxScore;
            entity.Grade = scale.Grade;
            entity.Remark = scale.Remark;
        }

        db.SaveChanges();

        // Refresh the cached grading scale used by the score-entry engine
        GradingService.ClearCache();

        Status.Text = "Grading scale saved. New scores will use the updated bands.";
        MessageBox.Show("Grading scale saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
