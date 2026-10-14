using EnergyGuard.Application.Interfaces;
using EnergyGuard.Application.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnergyReadingsController : ControllerBase
{
    private readonly IEnergyReadingService _service;

    public EnergyReadingsController(IEnergyReadingService service) => _service = service;

    [HttpGet("device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(Guid deviceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        Ok(await _service.GetByDeviceAsync(deviceId, page, pageSize));

    [HttpGet("range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10) =>
        Ok(await _service.GetReadingsByDateRangeAsync(start, end, page, pageSize));

    [HttpPost]
    public async Task<IActionResult> RegisterReading([FromBody] CreateEnergyReadingRequest request)
    {
        var result = await _service.RegisterReadingAsync(request);
        return Ok(result);
    }
}
