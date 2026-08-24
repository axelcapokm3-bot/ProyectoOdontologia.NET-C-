namespace ProyectoOdontologia.Tests.Fakes;

using global::CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using System.Threading;

/// <summary>
/// Fake manual de <see cref="IRepositorioInsumos"/>.
/// Reproduce el comportamiento del repositorio real (diccionario en memoria)
/// pero declarado dentro del proyecto de pruebas. Permite probar la capa de
/// aplicacion (servicios) sin depender de la infraestructura real ni de Moq.
/// </summary>
public class FakeRepositorioInsumos : IRepositorioInsumos
{
    private readonly Dictionary<int, Insumo> _insumos = new();
    private int _contadorId = 0;

    public Task<IEnumerable<Insumo>> ObtenerTodos()
    {
        return Task.FromResult<IEnumerable<Insumo>>(_insumos.Values);
    }

    public Task<Insumo?> ObtenerInsumoPorId(int id)
    {
        _insumos.TryGetValue(id, out Insumo? insumo);
        return Task.FromResult(insumo);
    }

    public Task<List<Insumo>> BuscarInsumos(string criterio)
    {
        List<Insumo> resultados = new List<Insumo>();

        if (string.IsNullOrWhiteSpace(criterio))
        {
            foreach (Insumo insumo in _insumos.Values)
            {
                resultados.Add(insumo);
            }

            return Task.FromResult(resultados);
        }

        string busqueda = criterio.Trim().ToLower();

        foreach (Insumo insumo in _insumos.Values)
        {
            string nombre = insumo.Nombre != null ? insumo.Nombre.ToLower() : "";
            string categoria = insumo.Categoria.ToString().ToLower();

            if (nombre.Contains(busqueda) || categoria.Contains(busqueda))
            {
                resultados.Add(insumo);
            }
        }

        return Task.FromResult(resultados);
    }

    public Task AgregarInsumo(Insumo insumo)
    {
        _insumos.Add(insumo.Id, insumo);
        return Task.CompletedTask;
    }

    public Task ActualizarInsumo(Insumo insumo)
    {
        if (!_insumos.ContainsKey(insumo.Id))
        {
            throw new KeyNotFoundException($"No se encontró un insumo con el ID {insumo.Id}.");
        }

        _insumos[insumo.Id] = insumo;
        return Task.CompletedTask;
    }

    public Task<bool> EliminarInsumo(int id)
    {
        return Task.FromResult(_insumos.Remove(id));
    }

    public int IdAutoIncrementoSincrono()
    {
        return Interlocked.Increment(ref _contadorId);
    }
}