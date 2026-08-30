using System.Threading.RateLimiting;
using ECommerceApi.Api.Middleware;
using ECommerceApi.Application;
using ECommerceApi.Application.Common.Constants;
using ECommerceApi.Infrastructure;
using ECommerceApi.Infrastructure.Identity;
using ECommerceApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Protege login/register contra brute-force e credential stuffing: no máximo
// 10 tentativas por minuto por IP, sem fila (excedente recebe 429 na hora).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Desligado por padrão: em desenvolvimento local as migrations são aplicadas
// via `dotnet ef database update`. O container Docker liga essa flag porque a
// imagem de runtime não tem a ferramenta dotnet-ef instalada.
if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
{
    await ApplyMigrationsAsync(app);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await SeedRolesAsync(app);

if (app.Environment.IsDevelopment())
{
    await SeedDevAdminAsync(app);
}

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}

static async Task SeedRolesAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { Roles.Admin, Roles.Customer })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Cria um usuário Admin local só em Development, para permitir testar endpoints
// administrativos (Swagger, Postman) sem precisar promover manualmente no banco.
static async Task SeedDevAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var email = config["DevSeedAdmin:Email"];
    var password = config["DevSeedAdmin:Password"];

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        return;
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (await userManager.FindByEmailAsync(email) is not null)
    {
        return;
    }

    var admin = new ApplicationUser { UserName = email, Email = email, FullName = "Administrador" };
    var result = await userManager.CreateAsync(admin, password);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
