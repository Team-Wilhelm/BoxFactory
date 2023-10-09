namespace Models.Util;

public class BoxParameters
{
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int BoxesPerPage { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool? Descending { get; set; }

}
