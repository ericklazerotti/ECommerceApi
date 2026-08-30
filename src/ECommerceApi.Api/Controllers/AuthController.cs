using ECommerceApi.Application.Common.Constants;
using ECommerceApi.Application.Common.Interfaces;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Infrastructure.Auth;
using ECommerceApi.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ECommerceApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        await _registerValidator.ValidateAndThrowAsync(dto);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Não foi possível registrar o usuário.", errors = result.Errors.Select(e => e.Description) });
        }

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        var token = _tokenService.GenerateToken(user.Id, user.Email!, new[] { Roles.Customer });
        return Ok(new AuthResponseDto(token, user.Email!, DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        await _loginValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);
        return Ok(new AuthResponseDto(token, user.Email!, DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)));
    }
}
