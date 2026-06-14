using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Services;
using SHSWassceResultsMgt.Views;

namespace SHSWassceResultsMgt;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionLabel.Text = $"v{version?.Major}.{version?.Minor}.{version?.Build}";

        Navigate("Dashboard");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
            Navigate(tag);
    }

    private void Navigate(string tag)
    {
        Page page = tag switch
        {
            "Dashboard" => new DashboardPage(),
            "Students"  => new StudentsPage(),
            "Subjects"  => new SubjectsPage(),
            "Scores"    => new ScoreEntryPage(),
            "Rankings"  => new RankingsPage(),
            "Reports"   => new ReportsPage(),
            "Staff"     => new StaffPage(),
            "Settings"  => new SettingsPage(),
            _           => new DashboardPage()
        };

        MainFrame.Navigate(page);
    }

    private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        => await UpdateService.CheckForUpdateAsync(silent: false);
}
