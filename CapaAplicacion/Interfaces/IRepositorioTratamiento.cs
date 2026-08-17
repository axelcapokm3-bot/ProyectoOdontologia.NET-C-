namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRepositorioTratamiento
{

    Task<IEnumerable<Tratamiento>> ObtenerTodos();

    Task<Tratamiento?> ObtenerTratamientoPorId(int id);

    Task<List<Tratamiento>> BuscarTratamientos(string criterio);


    Task AgregarTratamiento(Tratamiento tratamiento);

    Task ActualizarTratamiento(Tratamiento tratamiento);


    Task<bool> EliminarTratamiento(int id);
}