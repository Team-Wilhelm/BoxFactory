using System.ComponentModel.DataAnnotations;

namespace Models;

public class Dimensions
{
    public Guid Id { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Length { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Width { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public float Height { get; set; }
    
    public float Volume => Length * Width * Height;
    public float SurfaceArea => 2 * (Length * Width + Length * Height + Width * Height);
}