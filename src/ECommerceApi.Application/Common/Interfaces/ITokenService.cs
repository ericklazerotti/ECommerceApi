namespace ECommerceApi.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(string userId, string email, IList<string> roles);
}
