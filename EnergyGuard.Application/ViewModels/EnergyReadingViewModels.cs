namespace EnergyGuard.Application.ViewModels;

public class CreateEnergyReadingRequest
{
    public Guid DeviceId { get; set; }
    public decimal KilowattHour { get; set; }
    public DateTime ReadingDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class EnergyReadingResponse
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public decimal KilowattHour { get; set; }
    public DateTime ReadingDate { get; set; }
    public string Location { get; set; } = string.Empty;
}
