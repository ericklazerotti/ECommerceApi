using Microsoft.AspNetCore.Identity;

namespace ECommerceApi.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
