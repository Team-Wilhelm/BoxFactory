namespace Models;

public class Order
{
    public Guid Id { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Box>? Boxes { get; set; }
    public Customer? Customer { get; set; }
}