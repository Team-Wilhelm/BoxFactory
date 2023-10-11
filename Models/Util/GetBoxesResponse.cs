using Models.Models;

namespace Models.Util;

public class GetBoxesResponse
{
    public IEnumerable<Box> Boxes { get; set; }
    public int PageCount { get; set; }
}