namespace Models;

public class OrderCreateDto
{
    public Dictionary<Guid, int>? Boxes { get; set; }
    public Customer? Customer { get; set; }
    public ShippingStatus? ShippingStatus { get; set; }
}