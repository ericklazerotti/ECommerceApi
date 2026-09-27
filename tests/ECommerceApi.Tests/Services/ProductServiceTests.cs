using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Services;
using ECommerceApi.Domain.Entities;
using ECommerceApi.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerceApi.Tests.Services;

public class ProductServiceTests
{
    private static async Task<ApplicationDbContext> SeedProductsAsync(int count)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        var category = new Category { Name = "Eletrônicos" };
        context.Categories.Add(category);

        for (var i = 1; i <= count; i++)
        {
            context.Products.Add(new Product
            {
                Name = $"Produto {i:D2}",
                Price = 10m * i,
                StockQuantity = 5,
                CategoryId = category.Id,
                Category = category
            });
        }

        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsRequestedPageSize()
    {
        var context = await SeedProductsAsync(25);
        var sut = new ProductService(new UnitOfWork(context));

        var result = await sut.GetAllActiveAsync(new ProductQueryParameters { Page = 1, PageSize = 10 });

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAllActiveAsync_SecondPage_ReturnsRemainingItems()
    {
        var context = await SeedProductsAsync(25);
        var sut = new ProductService(new UnitOfWork(context));

        var result = await sut.GetAllActiveAsync(new ProductQueryParameters { Page = 3, PageSize = 10 });

        result.Items.Should().HaveCount(5);
        result.Page.Should().Be(3);
    }

    [Fact]
    public async Task GetAllActiveAsync_ClampsPageSizeAboveMaximum()
    {
        var context = await SeedProductsAsync(5);
        var sut = new ProductService(new UnitOfWork(context));

        var result = await sut.GetAllActiveAsync(new ProductQueryParameters { Page = 1, PageSize = 500 });

        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetAllActiveAsync_FiltersBySearchTerm_CaseInsensitive()
    {
        var context = await SeedProductsAsync(1);
        context.Products.Add(new Product { Name = "Mouse Gamer", Price = 99m, StockQuantity = 3, CategoryId = context.Categories.First().Id });
        await context.SaveChangesAsync();

        var sut = new ProductService(new UnitOfWork(context));

        var result = await sut.GetAllActiveAsync(new ProductQueryParameters { Search = "mouse" });

        result.Items.Should().ContainSingle(p => p.Name == "Mouse Gamer");
    }

    [Fact]
    public async Task GetAllActiveAsync_ExcludesInactiveProducts()
    {
        var context = await SeedProductsAsync(3);
        var inactive = context.Products.First();
        inactive.IsActive = false;
        await context.SaveChangesAsync();

        var sut = new ProductService(new UnitOfWork(context));

        var result = await sut.GetAllActiveAsync(new ProductQueryParameters { PageSize = 100 });

        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(p => p.Id == inactive.Id);
    }
}
