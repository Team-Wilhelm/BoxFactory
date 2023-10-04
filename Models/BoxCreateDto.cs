namespace Models;

public class BoxCreateDto
{
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    public DimensionsDto? DimensionsDto { get; set; }
    public int Stock { get; set; }
    public float Price { get; set; }
}