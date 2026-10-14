using EnergyGuard.Application.ViewModels;

namespace EnergyGuard.Application.Interfaces;

public interface IAlertService
{
    Task<PagedResult<AlertResponse>> GetActiveAlertsAsync(int page, int pageSize);
    Task<AlertResponse?> ResolveAlertAsync(Guid alertId);
    Task CheckAndGenerateAlertsAsync(Guid deviceId);
}
