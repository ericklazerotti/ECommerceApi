using System.Net;
using System.Text.Json;
using ECommerceApi.Application.Common.Exceptions;
using ECommerceApi.Domain.Exceptions;
using FluentValidation;

namespace ECommerceApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, (object?)null),
            InsufficientStockException => (HttpStatusCode.Conflict, exception.Message, (object?)null),
            BusinessRuleException => (HttpStatusCode.BadRequest, exception.Message, (object?)null),
            DomainException => (HttpStatusCode.BadRequest, exception.Message, (object?)null),
            ValidationException validationException => (HttpStatusCode.BadRequest, "Um ou mais erros de validação ocorreram.",
                (object?)validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })),
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.", (object?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado ao processar a requisição {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { message, errors }, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(payload);
    }
}
