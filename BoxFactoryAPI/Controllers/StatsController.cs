using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;

namespace BoxFactoryAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class StatsController : ControllerBase
{
    private readonly StatsService _statsService;

    public StatsController(StatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<ActionResult<Dictionary<int,int>>> Get()
    {
        return Ok(await _statsService.GetStats());
    }
}