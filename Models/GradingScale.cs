namespace SHSWassceResultsMgt.Models;

public class GradingScale
{
    public int Id { get; set; }
    public double MinScore { get; set; }
    public double MaxScore { get; set; }
    public int Grade { get; set; }
    public string Remark { get; set; } = string.Empty;
}
