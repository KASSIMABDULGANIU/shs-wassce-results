using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace SHSWassceResultsMgt.Services;

public static class UpdateService
{
    // ──────────────────────────────────────────────────────────
    //  STEP 1: Create a GitHub repo (public is simplest, free).
    //  STEP 2: Put version.json in the repo root, on the main branch.
    //  STEP 3: Use the "raw" URL format below — replace USERNAME/REPO.
    //
    //  Example raw URL format:
    //  https://raw.githubusercontent.com/USERNAME/REPO/main/version.json
    // ──────────────────────────────────────────────────────────
    private const string VersionUrl =
        "https://raw.githubusercontent.com/YOUR_USERNAME/shs-wassce-results/main/version.json";

    public static async Task CheckForUpdateAsync(bool silent = false)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            // Avoids GitHub's raw-content caching showing a stale version.json
            var url = $"{VersionUrl}?t={DateTimeOffset.UtcNow.Ticks}";
            var json = await http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var latestStr = root.GetProperty("version").GetString() ?? "0.0.0";
            var downloadUrl = root.GetProperty("downloadUrl").GetString() ?? string.Empty;
            var notes = root.TryGetProperty("notes", out var n) ? n.GetString() ?? "" : "";

            var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
            var latest = new Version(latestStr);

            if (latest > current)
            {
                var message = $"A new version ({latestStr}) is available!\n\nYou are on v{current.Major}.{current.Minor}.{current.Build}.";
                if (!string.IsNullOrWhiteSpace(notes))
                    message += $"\n\nWhat's new:\n{notes}";
                message += "\n\nDownload and install now?";

                var result = MessageBox.Show(message, "Update Available",
                    MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(downloadUrl))
                    Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
            }
            else if (!silent)
            {
                MessageBox.Show("You are on the latest version.", "No Updates",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch
        {
            // Offline or server unreachable — app continues to work fully offline.
            if (!silent)
                MessageBox.Show(
                    "Could not check for updates. Please check your internet connection.",
                    "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
