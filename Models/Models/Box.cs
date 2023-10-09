using System.ComponentModel.DataAnnotations;
using Models.Util;

namespace Models.Models;

public class Box
{
    public Guid Id { get; set; }
    
    [Required]
    [PositiveNumber]
    public float Weight { get; set; }
    
    public string? Colour { get; set; }
    public string? Material { get; set; }
    
    public Dimensions? Dimensions { get; set; }
    public DateTime CreatedAt { get; set; }
    
    [Required]
    [PositiveNumber]
    public int Stock { get; set; }
    
    [Required]
    [PositiveNumber]
    public float Price { get; set; }
}