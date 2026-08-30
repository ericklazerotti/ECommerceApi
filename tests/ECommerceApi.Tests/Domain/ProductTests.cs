using ECommerceApi.Domain.Entities;
using ECommerceApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ECommerceApi.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void DecreaseStock_WhenQuantityAvailable_ReducesStock()
    {
        var product = new Product { Name = "Mouse", StockQuantity = 10 };

        product.DecreaseStock(3);

        product.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void DecreaseStock_WhenQuantityExceedsStock_ThrowsInsufficientStockException()
    {
        var product = new Product { Name = "Teclado", StockQuantity = 2 };

        var act = () => product.DecreaseStock(5);

        act.Should().Throw<InsufficientStockException>();
        product.StockQuantity.Should().Be(2);
    }

    [Fact]
    public void IncreaseStock_AddsQuantityBack()
    {
        var product = new Product { Name = "Monitor", StockQuantity = 4 };

        product.IncreaseStock(6);

        product.StockQuantity.Should().Be(10);
    }
}
