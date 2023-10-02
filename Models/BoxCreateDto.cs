namespace Models;

public class BoxCreateDto
{
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    public float Price { get; set; }
    public Dimensions? Dimensions { get; set; }
}