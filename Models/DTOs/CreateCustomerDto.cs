using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class CreateCustomerDto
{
    [Required]
    [MinLength(1)]
    public string FirstName { get; set; }
    
    [Required]
    [MinLength(1)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public CreateAddressDto Address { get; set; }
    
    [Required]
    [MinLength(8)]
    public string PhoneNumber { get; set; }
}