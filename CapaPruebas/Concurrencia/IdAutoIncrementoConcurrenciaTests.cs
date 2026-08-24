namespace ProyectoOdontologia.Tests.Concurrencia;

using System.Collections.Concurrent;
using CapaInfraestructura.Implementacion;

/// <summary>
/// Pruebas de concurrencia y thread-safety de la generacion atomica de IDs
/// (patron Interlocked.Increment) de los cinco repositorios en memoria.
/// </summary>
public class IdAutoIncrementoConcurrenciaTests
{
    private const int CantidadIds = 1000;

    [Fact]
    public void IdAutoIncrementoSincrono_ParallelForGeneraMilIds_NoRepiteNingunId()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();
        ConcurrentBag<int> idsGenerados = new ConcurrentBag<int>();

        // Act
        Parallel.For(0, CantidadIds, _ =>
        {
            int id = repositorio.IdAutoIncrementoSincrono();
            idsGenerados.Add(id);
        });

        // Assert
        Assert.Equal(CantidadIds, idsGenerados.Count);
        Assert.Equal(CantidadIds, idsGenerados.Distinct().Count());
    }

    [Fact]
    public async Task IdAutoIncrementoSincrono_TaskWhenAllConcurrente_NoRepiteNingunId()
    {
        // Arrange
        RepositorioPaciente repositorio = new RepositorioPaciente();
        ConcurrentBag<int> idsGenerados = new ConcurrentBag<int>();

        // Act
        Task[] tareas = Enumerable.Range(0, CantidadIds)
            .Select(_ => Task.Run(() => idsGenerados.Add(repositorio.IdAutoIncrementoSincrono())))
            .ToArray();

        await Task.WhenAll(tareas);

        // Assert
        Assert.Equal(CantidadIds, idsGenerados.Count);
        Assert.Equal(CantidadIds, idsGenerados.Distinct().Count());
    }

    [Fact]
    public void IdAutoIncrementoSincrono_TodosLosRepositorios_GeneranIdsUnicosPorInstancia()
    {
        // Arrange
        RepositorioOdontologo odontologo = new RepositorioOdontologo();
        RepositorioTratamiento tratamiento = new RepositorioTratamiento();
        RepositorioTurnos turnos = new RepositorioTurnos();

        // Act
        int idOdontologo1 = odontologo.IdAutoIncrementoSincrono();
        int idTratamiento1 = tratamiento.IdAutoIncrementoSincrono();
        int idTurno1 = turnos.IdAutoIncrementoSincrono();

        // Assert
        Assert.Equal(1, idOdontologo1);
        Assert.Equal(1, idTratamiento1);
        Assert.Equal(1, idTurno1);
    }

    [Fact]
    public void IdAutoIncrementoSincrono_PrimeraInvocacionEnRepositorioVacio_RetornaUno()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act
        int primerId = repositorio.IdAutoIncrementoSincrono();

        // Assert
        Assert.Equal(1, primerId);
    }

    [Fact]
    public void IdAutoIncrementoSincrono_InvocacionesSecuenciales_SonConsecutivas()
    {
        // Arrange
        RepositorioInsumos repositorio = new RepositorioInsumos();

        // Act
        int primero = repositorio.IdAutoIncrementoSincrono();
        int segundo = repositorio.IdAutoIncrementoSincrono();
        int tercero = repositorio.IdAutoIncrementoSincrono();

        // Assert
        Assert.Equal(1, primero);
        Assert.Equal(2, segundo);
        Assert.Equal(3, tercero);
    }
}