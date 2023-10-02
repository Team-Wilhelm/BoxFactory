using Infrastructure;
using Models;

namespace Core;

public class BoxService
{
    private readonly BoxRepository _boxRepository;

    public BoxService(BoxRepository boxRepository)
    {
        _boxRepository = boxRepository;
    }

    public async Task<IEnumerable<Box>> Get()
    {
        return await _boxRepository.Get();
    }
    
    public async Task<Box> Get(Guid id)
    {
        return await _boxRepository.Get(id);
    }
    
    public async Task<Box> Create(BoxCreateDto boxCreateDto)
    {
        return await _boxRepository.Create(boxCreateDto);
    }
    
    public async Task<Box> Update(Guid id, BoxUpdateDto boxUpdateDto)
    {
        return await _boxRepository.Update(id, boxUpdateDto);
    }
    
    public async Task Delete(Guid id)
    {
        await _boxRepository.Delete(id);
    }
}