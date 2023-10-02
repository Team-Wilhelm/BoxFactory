using Models;

namespace Infrastructure;

public class BoxRepository
{
    public async Task<IEnumerable<Box>> Get()
    {
        throw new NotImplementedException();
    }
    
    public async Task<Box> Get(Guid id)
    {
        throw new NotImplementedException();
    }
    
    public async Task<Box> Create(BoxCreateDto boxCreateDto)
    {
        throw new NotImplementedException();
    }
    
    public async Task<Box> Update(Guid id, BoxUpdateDto boxUpdateDto)
    {
        throw new NotImplementedException();
    }
    
    public async Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}