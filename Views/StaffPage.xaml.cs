using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Views;

public partial class StaffPage : Page
{
    private readonly ObservableCollection<Staff> _list = new();
    private Staff? _selected;

    public StaffPage()
    {
        InitializeComponent();
        Grid.ItemsSource = _list;
        Loaded += (_, _) => Reload();
    }

    private void Reload()
    {
        using var db = new AppDbContext();
        _list.Clear();
        foreach (var s in db.Staff.OrderBy(s => s.Name))
            _list.Add(s);
    }

    private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = Grid.SelectedItem as Staff;
        if (_selected is null) return;
        NameBox.Text = _selected.Name;
        RoleBox.Text = _selected.Role;
        UsernameBox.Text = _selected.Username;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) ||
            string.IsNullOrWhiteSpace(UsernameBox.Text) ||
            string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBox.Show("Name, Username and Password are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new AppDbContext();

        if (db.Staff.Any(s => s.Username == UsernameBox.Text.Trim()))
        {
            MessageBox.Show("Username already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        db.Staff.Add(new Staff
        {
            Name = NameBox.Text.Trim(),
            Role = RoleBox.Text.Trim(),
            Username = UsernameBox.Text.Trim(),
            PasswordHash = Hash(PasswordBox.Password)
        });
        db.SaveChanges();

        Reload();
        Clear_Click(sender, e);
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            MessageBox.Show("Select a staff member to delete.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MessageBox.Show($"Delete {_selected.Name}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        using var db = new AppDbContext();
        db.Staff.Remove(db.Staff.Find(_selected.Id)!);
        db.SaveChanges();

        Reload();
        Clear_Click(sender, e);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        NameBox.Text = RoleBox.Text = UsernameBox.Text = string.Empty;
        PasswordBox.Password = string.Empty;
        _selected = null;
        Grid.SelectedItem = null;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
