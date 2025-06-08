namespace ApplicationCore.Models.Filtering;

public class LegoSetFilter
{
    public string? Name { get; set; }
    public string? Theme { get; set; }
    public string? Subtheme { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? MinPieces { get; set; }
    public int? MaxPieces { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
} 