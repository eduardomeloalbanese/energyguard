namespace EnergyGuard.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public decimal PowerLimitKwh { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<EnergyReading> Readings { get; set; } = new List<EnergyReading>();
}
