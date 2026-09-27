using ECommerceApi.Application.Common.Models;
using ECommerceApi.Application.DTOs;

namespace ECommerceApi.Application.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetAllActiveAsync(ProductQueryParameters query, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
