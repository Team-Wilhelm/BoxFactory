using AutoMapper;
using Infrastructure;
using Models;

namespace Core.Services;

public class BoxService
{
    private readonly BoxRepository _boxRepository;
    private readonly IMapper _mapper;

    public BoxService(BoxRepository boxRepository, IMapper mapper)
    {
        _boxRepository = boxRepository;
        _mapper = mapper;
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
        var box = _mapper.Map<Box>(boxCreateDto);
        box.CreatedAt = DateTime.Now;
        return await _boxRepository.Create(box);
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