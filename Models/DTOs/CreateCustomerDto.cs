using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class CreateCustomerDto
{
    [Required]
    [MinLength(1)]
    public string FirstName { get; set; } = null!;
    
    [Required]
    [MinLength(1)]
    public string LastName { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public CreateAddressDto Address { get; set; } = null!;
    
    [Required]
    [MinLength(8)]
    public string PhoneNumber { get; set; } = null!;
}