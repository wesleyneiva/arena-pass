using System.Net;
using System.Text.Json;
using ApplicationValidationException = ArenaPass.Application.Common.Exceptions.ValidationException;
using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Domain.Exceptions;

namespace ArenaPass.Api.Middleware;

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
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ApplicationValidationException validationException => (
                HttpStatusCode.BadRequest,
                validationException.Message,
                (object?)validationException.Erros),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                null),

            ConflitoDeAgendamentoException conflitoException => (
                HttpStatusCode.Conflict,
                conflitoException.Message,
                null),

            DomainException domainException => (
                HttpStatusCode.BadRequest,
                domainException.Message,
                null),

            UnauthorizedAccessException unauthorizedException => (
                HttpStatusCode.Unauthorized,
                unauthorizedException.Message,
                null),

            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado ao processar a requisição {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message, errors }));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseArenaPassExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
