namespace ECommerceApi.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId,
    string CategoryName);

public record CreateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId);

public record UpdateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    Guid CategoryId);
