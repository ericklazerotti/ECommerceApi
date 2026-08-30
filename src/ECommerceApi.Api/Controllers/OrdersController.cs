using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerceApi.Application.Common.Constants;
using ECommerceApi.Application.DTOs;
using ECommerceApi.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderDto> _createValidator;

    public OrdersController(IOrderService orderService, IValidator<CreateOrderDto> createValidator)
    {
        _orderService = orderService;
        _createValidator = createValidator;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(dto, cancellationToken);
        var created = await _orderService.CreateAsync(CurrentUserId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<OrderDto>>> GetMine(CancellationToken cancellationToken) =>
        Ok(await _orderService.ListByUserAsync(CurrentUserId, cancellationToken));

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<OrderDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _orderService.ListAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (!User.IsInRole(Roles.Admin) && order.UserId != CurrentUserId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<OrderDto>> MarkAsPaid(Guid id, CancellationToken cancellationToken) =>
        Ok(await _orderService.MarkAsPaidAsync(id, cancellationToken));

    [HttpPost("{id:guid}/ship")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<OrderDto>> MarkAsShipped(Guid id, CancellationToken cancellationToken) =>
        Ok(await _orderService.MarkAsShippedAsync(id, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (!User.IsInRole(Roles.Admin) && order.UserId != CurrentUserId)
        {
            return Forbid();
        }

        return Ok(await _orderService.CancelAsync(id, cancellationToken));
    }
}
