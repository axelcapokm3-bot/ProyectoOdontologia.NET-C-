namespace ProyectoOdontologia.Tests.Helpers;

/// <summary>
/// Helper para capturar excepciones lanzadas de forma SINCRONICA por metodos
/// que devuelven Task. Los repositorios en memoria validan parametros y lanzan
/// antes de construir el Task, por lo que el Assert.Throws() de xUnit es el
/// adecuado y el analizador xUnit2014 (pensado para excepciones asincronas)
/// genera un falso positivo en esos casos.
/// </summary>
public static class AssertionHelper
{
    public static TException ThrowsSync<TException>(Action accion) where TException : Exception
    {
        try
        {
            accion();
        }
        catch (TException ex)
        {
            return ex;
        }

        Assert.Fail($"Se esperaba la excepción {typeof(TException).Name} pero no se lanzó.");
        throw new InvalidOperationException("Código inalcanzable.");
    }
}