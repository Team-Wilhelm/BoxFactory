using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Models.DTOs;

public class OrderCreateDto
{
    [Required]
    public Dictionary<Guid, int> Boxes { get; set; } = new();
    
    [Required]
    public CreateCustomerDto Customer { get; set; } = null!;
}