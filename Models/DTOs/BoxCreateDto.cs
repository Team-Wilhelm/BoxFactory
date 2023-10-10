using System.ComponentModel.DataAnnotations;
using Models.Util;

namespace Models.DTOs;

public class BoxCreateDto
{
    [Required]
    [PositiveNumber]
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    
    [Required]
    public DimensionsDto? DimensionsDto { get; set; }
    
    [Required]
    [PositiveNumber]
    public int Stock { get; set; }
    
    [Required]
    [PositiveNumber]
    public float Price { get; set; }
}