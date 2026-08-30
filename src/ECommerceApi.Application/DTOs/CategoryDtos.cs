namespace ECommerceApi.Application.DTOs;

public record CategoryDto(Guid Id, string Name, string? Description, int ProductCount);

public record CreateCategoryDto(string Name, string? Description);

public record UpdateCategoryDto(string Name, string? Description);
