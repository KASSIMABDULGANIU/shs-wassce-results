using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Services;
using Microsoft.EntityFrameworkCore;

namespace SHSWassceResultsMgt;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        // Install global exception handlers so runtime errors are captured when
        // running from a terminal (useful for diagnosing silent exits).
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        try
        {
            base.OnStartup(e);

            // 1. Ensure DB exists and is created from the model (no migrations included)
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            // 2. Launch main window immediately (works offline)
            var window = new MainWindow();
            window.Show();

            // 3. Check for updates in background (only if internet available)
            await UpdateService.CheckForUpdateAsync(silent: true);
        }
        catch (Exception ex)
        {
            LogException(ex, "Unhandled exception during OnStartup");
            MessageBox.Show($"Startup error: {ex.Message}\nSee error.log in the application folder.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

    private static void LogException(Exception ex, string? context = null)
    {
        try
        {
            var text = $"[{DateTime.Now:u}] {context ?? "Exception"}\r\n{ex}\r\n\r\n";
            File.AppendAllText(LogFilePath, text);
        }
        catch
        {
            // Swallow logging errors to avoid secondary failures
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "DispatcherUnhandledException");
        e.Handled = true;
        MessageBox.Show($"Unhandled UI exception: {e.Exception.Message}\nSee error.log.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown(-1);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogException(ex, "CurrentDomain_UnhandledException");
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogException(e.Exception, "UnobservedTaskException");
        e.SetObserved();
    }
}
