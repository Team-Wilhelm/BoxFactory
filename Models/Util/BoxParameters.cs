namespace Models.Util;

public class BoxParameters
{
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int BoxesPerPage { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool? Descending { get; set; }
    public string? Filters { get; set; } // Passed in as Colour:Red,Blue;Material:Cardboard,Plastic;Weight:10-20...

    public Dictionary<FilterTypes, string> GetFilters()
    {
        var filters = new Dictionary<FilterTypes, string>();
        if (Filters == null) return filters;
        var filterStrings = Filters.Split(';');
        foreach (var filterString in filterStrings)
        {
            var filter = filterString.Split(':');
            filters.Add(Enum.Parse<FilterTypes>(filter[0]), filter[1]);
        }

        return filters;
    }
}
