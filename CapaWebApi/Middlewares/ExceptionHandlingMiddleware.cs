namespace CapaWebApi.Middlewares;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapaDominio.Excepciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Excepción de dominio capturada.");

            int statusCode;
            string title;

            switch (ex)
            {
                case TurnoSolapadoException:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Conflicto de agenda";
                    break;

                case PacienteNoEncontradoException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Paciente no encontrado";
                    break;

                default:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Regla de negocio incumplida";
                    break;
            }

            await EscribirProblemDetailsAsync(context, statusCode, title, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Recurso no encontrado.");
            await EscribirProblemDetailsAsync(
                context,
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Petición inválida.");
            await EscribirProblemDetailsAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Petición inválida",
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflicto de regla de negocio.");
            await EscribirProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflicto de regla de negocio",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            await EscribirProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado.");
        }
    }

    private static async Task EscribirProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
            return;

        ProblemDetails problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}
