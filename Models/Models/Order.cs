namespace Models.Models;

public class Order
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<Guid, int> Boxes { get; set; } = new();
    public Customer? Customer { get; set; }
    public ShippingStatus ShippingStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalBoxes { get; set; }
}