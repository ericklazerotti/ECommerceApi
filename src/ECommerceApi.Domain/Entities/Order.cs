using ECommerceApi.Domain.Enums;
using ECommerceApi.Domain.Exceptions;

namespace ECommerceApi.Domain.Entities;

public class Order : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public decimal TotalAmount => Items.Sum(i => i.Total);

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException($"Pedido não pode ser pago a partir do status '{Status}'.");
        }

        Status = OrderStatus.Paid;
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
        {
            throw new DomainException($"Pedido não pode ser enviado a partir do status '{Status}'.");
        }

        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Cancelled)
        {
            throw new DomainException($"Pedido não pode ser cancelado a partir do status '{Status}'.");
        }

        Status = OrderStatus.Cancelled;
    }
}
