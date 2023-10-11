using Infrastructure;
using Models.DTOs;

namespace Core.Services;

public class StatsService
{
    private readonly StatsRepository _statsRepository;
    
    public StatsService(StatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }
    
    public async Task<Dictionary<int, int>> GetStats()
    {
        try
        {
            return await _statsRepository.GetStats();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message, e.InnerException, "Error getting stats");
            throw;
        }
    }
}