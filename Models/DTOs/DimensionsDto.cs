using System.ComponentModel.DataAnnotations;
using Models.Util;

namespace Models.DTOs;

public class DimensionsDto
{
    [Required]
    [PositiveNumber]
    public float Length { get; set; }
    
    [Required]
    [PositiveNumber]
    public float Width { get; set; }
    
    [Required]
    [PositiveNumber]
    public float Height { get; set; }
}