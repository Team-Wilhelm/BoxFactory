using AutoMapper;
using Infrastructure;
using Models;
using Models.Exceptions;

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

    public async Task<IEnumerable<Box>> Get(string? searchTerm, int currentPage, int boxesPerPage)
    {
        return await _boxRepository.Get(searchTerm, currentPage, boxesPerPage);
    }

    public async Task<Box> Get(Guid id)
    {
        try
        {
            return await _boxRepository.Get(id);
        }
        catch (InvalidOperationException)
        {
            throw new NotFoundException("Box not found");
        }
    }

    public async Task<Box> Create(BoxCreateDto boxCreateDto)
    {
        var box = _mapper.Map<Box>(boxCreateDto);
        box.CreatedAt = DateTime.Now;
        return await _boxRepository.Create(box);
    }

    public async Task<Box> Update(Guid id, BoxUpdateDto boxUpdateDto)
    {
        var box = await Get(id);
        box = _mapper.Map(boxUpdateDto, box);
        return await _boxRepository.Update(box);
    }

    public async Task Delete(Guid id)
    {
        await Get(id);
        await _boxRepository.Delete(id);
    }
}