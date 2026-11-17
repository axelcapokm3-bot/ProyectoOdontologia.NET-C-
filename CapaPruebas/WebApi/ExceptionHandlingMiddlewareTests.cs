namespace ProyectoOdontologia.Tests.WebApi;

using System.Text.Json;
using CapaDominio.Excepciones;
using CapaWebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<ResultadoMiddleware> EjecutarAsync(Exception excepcion)
    {
        DefaultHttpContext contexto = new DefaultHttpContext();
        contexto.Request.Path = "/api/turnos";
        contexto.Response.Body = new MemoryStream();

        ExceptionHandlingMiddleware middleware = new ExceptionHandlingMiddleware(
            delegate { return Task.FromException(excepcion); },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(contexto);

        contexto.Response.Body.Position = 0;
        ProblemDetails? problema = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            contexto.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return new ResultadoMiddleware(contexto, problema);
    }

    [Fact]
    public async Task InvokeAsync_TurnoSolapado_Responde409()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new TurnoSolapadoException("El turno se solapa con otro turno existente."));

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Contexto.Response.StatusCode);
        Assert.Equal("application/problem+json", resultado.Contexto.Response.ContentType);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Problema!.Status);
    }

    [Fact]
    public async Task InvokeAsync_PacienteNoEncontrado_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new PacienteNoEncontradoException(99));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Contains("99", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_OdontologoNoEncontrado_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new OdontologoNoEncontradoException(44));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Contains("44", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_TratamientoNoEncontrado_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new TratamientoNoEncontradoException(55));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Contains("55", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_TurnoNoEncontrado_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new TurnoNoEncontradoException(77));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Equal("Turno no encontrado", resultado.Problema.Title);
        Assert.Contains("77", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_InsumoNoEncontrado_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new InsumoNoEncontradoException(66));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Contains("66", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_StockInsuficiente_Responde409()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(new StockInsuficienteException(7, 20, 5));

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Problema!.Status);
        Assert.Contains("7", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_DomainExceptionGenerica_Responde400()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new DomainException("Regla de negocio incumplida."));

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status400BadRequest, resultado.Problema!.Status);
    }

    [Fact]
    public async Task InvokeAsync_TurnoNoModificable_Responde409()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new TurnoNoModificableException("Solo se pueden modificar turnos en estado Pendiente."));

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Contexto.Response.StatusCode);
        Assert.Equal("application/problem+json", resultado.Contexto.Response.ContentType);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status409Conflict, resultado.Problema!.Status);
        Assert.Equal("Turno no modificable", resultado.Problema.Title);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccess_Responde401()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new UnauthorizedAccessException("Credenciales inválidas."));

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, resultado.Contexto.Response.StatusCode);
        Assert.Equal("application/problem+json", resultado.Contexto.Response.ContentType);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status401Unauthorized, resultado.Problema!.Status);
        Assert.Equal("No autorizado", resultado.Problema.Title);
        Assert.Contains("Credenciales", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFound_Responde404()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new KeyNotFoundException("No se encontró un turno con el ID 99."));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Contexto.Response.StatusCode);
        Assert.Equal("application/problem+json", resultado.Contexto.Response.ContentType);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status404NotFound, resultado.Problema!.Status);
        Assert.Equal("Recurso no encontrado", resultado.Problema.Title);
        Assert.Contains("99", resultado.Problema.Detail);
    }

    [Fact]
    public async Task InvokeAsync_ExcepcionNoControlada_Responde500SinExponerElDetalle()
    {
        // Act
        ResultadoMiddleware resultado = await EjecutarAsync(
            new Exception("detalle sensible que no debe exponerse"));

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, resultado.Contexto.Response.StatusCode);
        Assert.NotNull(resultado.Problema);
        Assert.Equal(StatusCodes.Status500InternalServerError, resultado.Problema!.Status);
        Assert.Equal("Ocurrió un error inesperado.", resultado.Problema.Detail);
        Assert.DoesNotContain("detalle sensible", resultado.Problema.Detail);
    }

    private sealed class ResultadoMiddleware
    {
        public ResultadoMiddleware(DefaultHttpContext contexto, ProblemDetails? problema)
        {
            Contexto = contexto;
            Problema = problema;
        }

        public DefaultHttpContext Contexto { get; }
        public ProblemDetails? Problema { get; }
    }
}
