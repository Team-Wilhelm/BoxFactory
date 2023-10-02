namespace Models;

public class Box
{
    public Guid Id { get; set; }
    public float Weight { get; set; }
    public string? Colour { get; set; }
    public string? Material { get; set; }
    public Dimensions? Dimensions { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Stock { get; set; }
    public float Price { get; set; }
}