using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TuProyecto.WebAPI.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Excepción capturada: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            ArgumentException or InvalidOperationException =>
                (StatusCodes.Status400BadRequest, "Regla de Negocio / Petición Inválida"),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Recurso No Encontrado"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Error Interno del Servidor")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}