using System.Net;
using System.Text.Json;
using FluentValidation;

namespace IntergalaxyTech.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió una excepción no controlada.");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Error interno del servidor.";

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Error de validación: " + string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage));
                break;
            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = argumentException.Message;
                break;
            case InvalidOperationException invalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = invalidOperationException.Message;
                break;
            case KeyNotFoundException keyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = keyNotFoundException.Message;
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
