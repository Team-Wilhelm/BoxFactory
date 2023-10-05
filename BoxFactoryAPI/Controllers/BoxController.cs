using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace BoxFactoryAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class BoxController : ControllerBase
{
    private readonly BoxService _boxService;

    public BoxController(BoxService boxService)
    {
        _boxService = boxService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Box>>> Get([FromQuery] string? searchTerm, [FromQuery] int currentPage, 
        [FromQuery] int boxesPerPage, [FromQuery] string? sortBy, [FromQuery] bool? descending)
    {
        var sorting = new Sorting(sortBy, descending);
        return Ok(await _boxService.Get(searchTerm, currentPage, boxesPerPage, sorting));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Box>> Get([FromRoute] Guid id)
    {
        return Ok(await _boxService.Get(id));
    }

    [HttpPost]
    public async Task<ActionResult<Box>> Create([FromBody] BoxCreateDto boxCreateDto)
    {
        return Ok(await _boxService.Create(boxCreateDto));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Box>> Update([FromRoute] Guid id, [FromBody] BoxUpdateDto boxUpdateDto)
    {
        return Ok(await _boxService.Update(id, boxUpdateDto));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _boxService.Delete(id);
        return Ok();
    }
}