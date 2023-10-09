namespace Models.Util;

public class Sorting
{
    public string? SortBy { get; set; }
    public bool? Descending { get; set; }
    public string Query { get; set; }
    
    public Sorting(string? sortBy, bool? descending)
    {
        SortBy = sortBy;
        Descending = descending;
        Query = sortBy == null ? "" : $"ORDER BY {sortBy} {(descending == true ? "DESC" : "ASC")}";
    }
}