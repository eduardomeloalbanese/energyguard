using EnergyGuard.Domain.Enums;

namespace EnergyGuard.Application.ViewModels;

public class AlertResponse
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlertType AlertType { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ResolveAlertRequest
{
    public Guid AlertId { get; set; }
}
