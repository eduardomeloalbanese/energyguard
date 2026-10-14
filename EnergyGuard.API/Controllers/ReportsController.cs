using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        var result = await _reportService.GenerateReportAsync(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await _reportService.GetReportsAsync(page, pageSize));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        var result = await _reportService.GetReportByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
