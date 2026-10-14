using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using EnergyGuard.Domain.Entities;

namespace EnergyGuard.Application.Services;

public class ReportService : IReportService
{
    private readonly IRepository<EnergyReport> _repository;
    private readonly IEnergyReadingQueryRepository _readingQueryRepository;

    public ReportService(IRepository<EnergyReport> repository, IEnergyReadingQueryRepository readingQueryRepository)
    {
        _repository = repository;
        _readingQueryRepository = readingQueryRepository;
    }

    public async Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request)
    {
        var readings = (await _readingQueryRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd)).ToList();

        var total = readings.Sum(r => r.KilowattHour);
        var average = readings.Count > 0 ? readings.Average(r => r.KilowattHour) : 0m;
        var peak = readings.Count > 0 ? readings.Max(r => r.KilowattHour) : 0m;

        var report = new EnergyReport
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TotalKwh = total,
            AverageKwh = average,
            PeakKwh = peak,
            CarbonFootprintKg = total * 0.233m,
            GeneratedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(report);
        await _repository.SaveChangesAsync();
        return MapToResponse(report);
    }

    public async Task<PagedResult<ReportResponse>> GetReportsAsync(int page, int pageSize)
    {
        var (items, total) = await _repository.GetAllPagedAsync(page, pageSize);
        return new PagedResult<ReportResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ReportResponse?> GetReportByIdAsync(Guid id)
    {
        var report = await _repository.GetByIdAsync(id);
        return report is null ? null : MapToResponse(report);
    }

    private static ReportResponse MapToResponse(EnergyReport r) => new()
    {
        Id = r.Id,
        Title = r.Title,
        PeriodStart = r.PeriodStart,
        PeriodEnd = r.PeriodEnd,
        TotalKwh = r.TotalKwh,
        AverageKwh = r.AverageKwh,
        PeakKwh = r.PeakKwh,
        CarbonFootprintKg = r.CarbonFootprintKg,
        GeneratedAt = r.GeneratedAt
    };
}
