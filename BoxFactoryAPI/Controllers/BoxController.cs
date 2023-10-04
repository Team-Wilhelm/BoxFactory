using System.Collections;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Models;

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
    public async Task<ActionResult<IEnumerable<Box>>> Get()
    {
        return Ok(await _boxService.Get());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Box>> Get([FromRoute] Guid id)
    {
        return await _boxService.Get(id);
    }

    [HttpPost]
    public async Task<ActionResult<Box>> Create([FromBody] BoxCreateDto boxCreateDto)
    {
        return await _boxService.Create(boxCreateDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Box>> Update([FromRoute] Guid id, [FromBody] BoxUpdateDto boxUpdateDto)
    {
        return await _boxService.Update(id, boxUpdateDto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _boxService.Delete(id);
        return Ok();
    }
}