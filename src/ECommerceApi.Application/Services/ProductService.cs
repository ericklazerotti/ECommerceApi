using ECommerceApi.Application.Common.Exceptions;
using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Mapping;
using ECommerceApi.Application.Services.Interfaces;
using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.ListActiveAsync(cancellationToken);
        return products.ToDto();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return product.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), dto.CategoryId);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId
        };

        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id, cancellationToken);
        return created!.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        _ = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), dto.CategoryId);

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Products.GetByIdWithCategoryAsync(id, cancellationToken);
        return updated!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        // Soft delete: pedidos existentes referenciam o produto, então ele é
        // desativado em vez de removido para preservar o histórico de pedidos.
        product.IsActive = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
