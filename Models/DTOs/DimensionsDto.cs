using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class DimensionsDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public float Length { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Width { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Height { get; set; }
}