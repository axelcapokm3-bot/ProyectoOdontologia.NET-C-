namespace CapaWebApi.Middlewares;

using System;
using System.Threading.Tasks;
using CapaDominio.Excepciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
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

                case OdontologoNoEncontradoException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Odontólogo no encontrado";
                    break;

                case TratamientoNoEncontradoException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Tratamiento no encontrado";
                    break;

                case TurnoNoEncontradoException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Turno no encontrado";
                    break;

                case InsumoNoEncontradoException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Insumo no encontrado";
                    break;

                case StockInsuficienteException:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Stock insuficiente";
                    break;

                default:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Regla de negocio incumplida";
                    break;
            }

            await EscribirProblemDetailsAsync(context, statusCode, title, ex.Message);
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
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            _logger.LogWarning(ex, "Conflicto de integridad de datos.");
            await EscribirProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflicto de integridad de datos",
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
