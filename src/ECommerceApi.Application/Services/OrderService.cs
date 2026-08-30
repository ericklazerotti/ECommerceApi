using ECommerceApi.Application.Common.Exceptions;
using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Mapping;
using ECommerceApi.Application.Services.Interfaces;
using ECommerceApi.Domain.Entities;

namespace ECommerceApi.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> CreateAsync(string userId, CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = new Order { UserId = userId };

        foreach (var item in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            if (!product.IsActive)
            {
                throw new BusinessRuleException($"Produto '{product.Name}' não está mais disponível.");
            }

            // Regra de negócio central: nunca permitir vender além do estoque disponível.
            product.DecreaseStock(item.Quantity);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });
        }

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Orders.GetByIdWithItemsAsync(order.Id, cancellationToken);
        return created!.ToDto();
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        return order.ToDto();
    }

    public async Task<List<OrderDto>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.ListByUserAsync(userId, cancellationToken);
        return orders.ToDto();
    }

    public async Task<List<OrderDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.ListAllWithItemsAsync(cancellationToken);
        return orders.ToDto();
    }

    public async Task<OrderDto> MarkAsPaidAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.MarkAsPaid();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    public async Task<OrderDto> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.MarkAsShipped();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Cancel();

        // Devolve o estoque reservado pelo pedido cancelado.
        foreach (var item in order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            product?.IncreaseStock(item.Quantity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }
}
