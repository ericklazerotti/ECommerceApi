using ECommerceApi.Domain.Exceptions;

namespace ECommerceApi.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public void DecreaseStock(int quantity)
    {
        if (quantity > StockQuantity)
        {
            throw new InsufficientStockException(Name, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        StockQuantity += quantity;
    }
}
