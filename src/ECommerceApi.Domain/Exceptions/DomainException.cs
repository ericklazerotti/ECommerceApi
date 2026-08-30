namespace ECommerceApi.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int requested, int available)
        : base($"Estoque insuficiente para '{productName}'. Solicitado: {requested}, disponível: {available}.")
    {
    }
}
