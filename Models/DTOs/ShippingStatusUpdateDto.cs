using System.ComponentModel.DataAnnotations;
using Models.Models;

namespace Models.DTOs;

public class ShippingStatusUpdateDto
{
    [Required]
    public ShippingStatus ShippingStatus { get; set; }
}