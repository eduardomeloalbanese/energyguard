namespace EnergyGuard.Domain.Entities;

public class EnergyReading
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public decimal KilowattHour { get; set; }
    public DateTime ReadingDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Device Device { get; set; } = null!;
}
