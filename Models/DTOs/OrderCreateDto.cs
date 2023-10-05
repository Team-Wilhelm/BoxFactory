namespace Models.DTOs;

public class OrderCreateDto
{
    public Dictionary<Guid, int>? Boxes { get; set; }
    public Customer? Customer { get; set; }
}