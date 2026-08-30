using ECommerceApi.Application.Common.Exceptions;
using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Mapping;
using ECommerceApi.Application.Services.Interfaces;
using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.ListAsync(cancellationToken);
        return categories.ToDto();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        var hasProducts = await _unitOfWork.Products.ListAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts.Count > 0)
        {
            throw new BusinessRuleException("Não é possível excluir uma categoria que possui produtos vinculados.");
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
