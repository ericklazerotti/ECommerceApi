using ECommerceApi.Application.DTOs;
using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Mapping;

public static class MappingExtensions
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.IsActive,
        product.CategoryId,
        product.Category?.Name ?? string.Empty);

    public static List<ProductDto> ToDto(this IEnumerable<Product> products) =>
        products.Select(p => p.ToDto()).ToList();

    public static CategoryDto ToDto(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Products.Count);

    public static List<CategoryDto> ToDto(this IEnumerable<Category> categories) =>
        categories.Select(c => c.ToDto()).ToList();

    public static OrderItemDto ToDto(this OrderItem item) => new(
        item.ProductId,
        item.ProductName,
        item.UnitPrice,
        item.Quantity,
        item.Total);

    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.UserId,
        order.Status,
        order.TotalAmount,
        order.CreatedAtUtc,
        order.Items.Select(i => i.ToDto()).ToList());

    public static List<OrderDto> ToDto(this IEnumerable<Order> orders) =>
        orders.Select(o => o.ToDto()).ToList();
}
