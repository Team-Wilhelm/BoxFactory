using System.ComponentModel.DataAnnotations;
using Models.Util;

namespace Models.DTOs;

public class CreateAddressDto
{
    [Required]
    [MinLength(1)]
    public string StreetName { get; set; } = null!;

    [Required] [PositiveNumber] 
    public int HouseNumber { get; set; }
    
    public string HouseNumberAddition { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1)]
    public string City { get; set; } = null!;
    
    [Required]
    [MinLength(1)]
    public string Country { get; set; } = null!;
    
    [Required]
    [MinLength(1)]
    public string PostalCode { get; set; } = null!;
}