using EnergyGuard.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService) => _alertService = alertService;

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveAlerts([FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await _alertService.GetActiveAlertsAsync(page, pageSize));

    [HttpPut("{id:guid}/resolve")]
    public async Task<IActionResult> ResolveAlert(Guid id)
    {
        var result = await _alertService.ResolveAlertAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
