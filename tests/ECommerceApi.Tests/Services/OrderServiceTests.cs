using ECommerceApi.Application.Common.Exceptions;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Services;
using ECommerceApi.Domain.Entities;
using ECommerceApi.Domain.Exceptions;
using ECommerceApi.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerceApi.Tests.Services;

public class OrderServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(ApplicationDbContext Context, Category Category, Product Product)> SeedProductAsync(int stock = 10)
    {
        var context = CreateContext();
        var category = new Category { Name = "Eletrônicos" };
        var product = new Product { Name = "Teclado Mecânico", Price = 250m, StockQuantity = stock, CategoryId = category.Id, Category = category };

        context.Categories.Add(category);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return (context, category, product);
    }

    [Fact]
    public async Task CreateAsync_WithEnoughStock_CreatesOrderAndDecreasesStock()
    {
        var (context, _, product) = await SeedProductAsync(stock: 10);
        var unitOfWork = new UnitOfWork(context);
        var sut = new OrderService(unitOfWork);

        var dto = new CreateOrderDto(new List<CreateOrderItemDto> { new(product.Id, 3) });
        var result = await sut.CreateAsync("user-1", dto);

        result.TotalAmount.Should().Be(750m);
        result.Items.Should().ContainSingle(i => i.ProductId == product.Id && i.Quantity == 3);

        var updatedProduct = await context.Products.FindAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task CreateAsync_WithoutEnoughStock_ThrowsAndDoesNotPersistOrder()
    {
        var (context, _, product) = await SeedProductAsync(stock: 2);
        var unitOfWork = new UnitOfWork(context);
        var sut = new OrderService(unitOfWork);

        var dto = new CreateOrderDto(new List<CreateOrderItemDto> { new(product.Id, 5) });
        var act = async () => await sut.CreateAsync("user-1", dto);

        await act.Should().ThrowAsync<InsufficientStockException>();
        context.Orders.Should().BeEmpty();

        var untouchedProduct = await context.Products.FindAsync(product.Id);
        untouchedProduct!.StockQuantity.Should().Be(2);
    }

    [Fact]
    public async Task CreateAsync_ForInactiveProduct_ThrowsBusinessRuleException()
    {
        var (context, _, product) = await SeedProductAsync(stock: 5);
        product.IsActive = false;
        await context.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(context);
        var sut = new OrderService(unitOfWork);

        var dto = new CreateOrderDto(new List<CreateOrderItemDto> { new(product.Id, 1) });
        var act = async () => await sut.CreateAsync("user-1", dto);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task CancelAsync_RestoresStockToProducts()
    {
        var (context, _, product) = await SeedProductAsync(stock: 10);
        var unitOfWork = new UnitOfWork(context);
        var sut = new OrderService(unitOfWork);

        var created = await sut.CreateAsync("user-1", new CreateOrderDto(new List<CreateOrderItemDto> { new(product.Id, 4) }));

        await sut.CancelAsync(created.Id);

        var restoredProduct = await context.Products.FindAsync(product.Id);
        restoredProduct!.StockQuantity.Should().Be(10);
    }
}
