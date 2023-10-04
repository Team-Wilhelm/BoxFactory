namespace Models;

public class OrderCreateDto
{
    public List<Box>? Boxes { get; set; }
    public Customer? Customer { get; set; }
    public ShippingStatus? ShippingStatus { get; set; }
}