namespace EnergyGuard.Domain.Entities;

public class EnergyReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalKwh { get; set; }
    public decimal AverageKwh { get; set; }
    public decimal PeakKwh { get; set; }
    public decimal CarbonFootprintKg { get; set; }
    public DateTime GeneratedAt { get; set; }
}
