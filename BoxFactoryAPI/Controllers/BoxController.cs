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
    public async Task<IEnumerable<Box>> Get()
    {
        return await _boxService.Get();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<Box> Get(Guid id)
    {
        return await _boxService.Get(id);
    }
    
    [HttpPost]
    public async Task<Box> Create(BoxCreateDto boxCreateDto)
    {
        return await _boxService.Create(boxCreateDto);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<Box> Update(Guid id, BoxUpdateDto boxUpdateDto)
    {
        return await _boxService.Update(id, boxUpdateDto);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id)
    {
        await _boxService.Delete(id);
    }
}