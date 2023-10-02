namespace Models;

public class Dimensions
{
    public float Length { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Volume => Length * Width * Height;
    public float SurfaceArea => 2 * (Length * Width + Length * Height + Width * Height);
}