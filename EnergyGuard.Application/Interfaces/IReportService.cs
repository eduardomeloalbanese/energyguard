using EnergyGuard.Application.ViewModels;

namespace EnergyGuard.Application.Interfaces;

public interface IReportService
{
    Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request);
    Task<PagedResult<ReportResponse>> GetReportsAsync(int page, int pageSize);
    Task<ReportResponse?> GetReportByIdAsync(Guid id);
}
