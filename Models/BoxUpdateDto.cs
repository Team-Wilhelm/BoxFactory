namespace Models;

public class BoxUpdateDto
{
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    public Dimensions? Dimensions { get; set; }
}