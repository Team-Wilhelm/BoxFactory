namespace Models;

public class OrderCreateDto
{
    public List<Guid>? Boxes { get; set; }
    public Customer? Customer { get; set; }
    public ShippingStatus? ShippingStatus { get; set; }
}