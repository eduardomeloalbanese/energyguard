namespace EnergyGuard.Application.ViewModels;

public class CreateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PowerLimitKwh { get; set; }
}

public class UpdateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PowerLimitKwh { get; set; }
    public bool IsActive { get; set; }
}

public class DeviceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal PowerLimitKwh { get; set; }
    public DateTime CreatedAt { get; set; }
}
