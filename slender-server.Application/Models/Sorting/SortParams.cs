namespace slender_server.Application.Models.Sorting;

public sealed class SortParams
{
    /// <summary>
    /// Sort string format: "field1 asc, field2 desc, field3"
    /// Example: "title asc, createdAt desc"
    /// </summary>
    public string? Sort { get; set; }
}