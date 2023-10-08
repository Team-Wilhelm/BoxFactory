namespace Models;

public class CreateAddressDto
{
    public string? StreetName { get; set; }
    public int HouseNumber { get; set; }
    public string HouseNumberAddition { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}