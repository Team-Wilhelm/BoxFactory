namespace Models.DTOs;

public class CreateCustomerDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public CreateAddressDto? Address { get; set; }
    public string? PhoneNumber { get; set; }
}