using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class ShippingStatusUpdateDto
{
    [Required]
    public ShippingStatus ShippingStatus { get; set; }
}