namespace Models.Models;

public class Customer
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public Address? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public List<Order>? Orders { get; set; }
}