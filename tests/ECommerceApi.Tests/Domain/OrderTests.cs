using ECommerceApi.Domain.Entities;
using ECommerceApi.Domain.Enums;
using ECommerceApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace ECommerceApi.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void TotalAmount_SumsAllItems()
    {
        var order = new Order
        {
            Items =
            {
                new OrderItem { UnitPrice = 10m, Quantity = 2 },
                new OrderItem { UnitPrice = 5m, Quantity = 3 }
            }
        };

        order.TotalAmount.Should().Be(35m);
    }

    [Fact]
    public void MarkAsPaid_FromPending_TransitionsToPaid()
    {
        var order = new Order();

        order.MarkAsPaid();

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenNotPending_ThrowsDomainException()
    {
        var order = new Order { Status = OrderStatus.Shipped };

        var act = () => order.MarkAsPaid();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_WhenShipped_ThrowsDomainException()
    {
        var order = new Order { Status = OrderStatus.Shipped };

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>();
    }
}
