using SHSWassceResultsMgt.Data;
using SHSWassceResultsMgt.Models;

namespace SHSWassceResultsMgt.Services;

public static class GradingService
{
    private static List<GradingScale>? _cache;

    public static (int grade, string remark) Compute(double classScore, double examScore)
    {
        _cache ??= LoadScales();
        var total = Math.Round(classScore + examScore, 2);
        var band = _cache.FirstOrDefault(s => total >= s.MinScore && total <= s.MaxScore);
        return (band?.Grade ?? 9, band?.Remark ?? "Fail");
    }

    public static void ClearCache() => _cache = null;

    private static List<GradingScale> LoadScales()
    {
        using var db = new AppDbContext();
        return db.GradingScales.OrderByDescending(g => g.MinScore).ToList();
    }
}
