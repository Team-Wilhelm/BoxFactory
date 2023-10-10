using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Models.DTOs;

public class CreateAddressDto
{
    [Required]
    [MinLength(1)]
    public string StreetName { get; set; }
    
    [Required]
    [PositiveNumber]
    public int HouseNumber { get; set; }
    
    public string HouseNumberAddition { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    public string City { get; set; }
    
    [Required]
    [MinLength(1)]
    public string Country { get; set; }
    
    [Required]
    [MinLength(1)]
    public string PostalCode { get; set; }
}