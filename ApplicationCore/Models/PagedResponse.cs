namespace ApplicationCore.Models;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }
    public PaginationMetadata Metadata { get; set; }

    public PagedResponse(IEnumerable<T> items, PaginationMetadata metadata)
    {
        Items = items;
        Metadata = metadata;
    }
} 