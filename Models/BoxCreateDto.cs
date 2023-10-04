using System.ComponentModel.DataAnnotations;

namespace Models;

public class BoxCreateDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    
    [Required]
    public DimensionsDto? DimensionsDto { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Price { get; set; }
}