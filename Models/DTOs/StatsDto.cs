namespace Models.DTOs;

public class StatsDto
{
    public Dictionary<int, int> OrdersPerMonth { get; set; } = new();
}