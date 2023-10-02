using Microsoft.AspNetCore.Mvc;
using Models;

namespace BoxFactoryAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class BoxController : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<Box>> Get()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<Box> Get(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<Box> Post(Box box)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id:guid}")]
    public async Task<Box> Put(Guid id, Box box)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}