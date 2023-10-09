namespace Models.Util;

public class BoxFilters
{
    // Filtering
    public List<string> Colours { get; set; } = new ();
    public List<string> Materials { get; set; } = new ();
    public int MinPrice { get; set; } = 0;
    public int MaxPrice { get; set; }
    
    public int MinWeight { get; set; } = 0;
    public int MaxWeight { get; set; }
    public int MinVolume { get; set; } = 0;
    public int MaxVolume { get; set; }
}