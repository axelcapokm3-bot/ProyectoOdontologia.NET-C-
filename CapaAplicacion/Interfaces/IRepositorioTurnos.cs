namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRepositorioTurnos
{

    Task<IEnumerable<Turno>> ObtenerTodos();
    Task<Turno?> ObtenerTurnoPorId(int id);

    Task<List<Turno>> BuscarTurnos(string criterio);


    Task AgregarTurno(Turno turno);


    Task ActualizarTurno(Turno turno);

    Task<bool> EliminarTurno(int id);
}