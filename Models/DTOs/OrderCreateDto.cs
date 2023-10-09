namespace Models.DTOs;

public class OrderCreateDto
{
    public Dictionary<Guid, int> Boxes { get; set; } = new();
    public CreateCustomerDto? Customer { get; set; }
}