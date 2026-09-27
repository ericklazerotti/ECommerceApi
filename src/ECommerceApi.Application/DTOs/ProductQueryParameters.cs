namespace ECommerceApi.Application.DTOs;

public class ProductQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
}
