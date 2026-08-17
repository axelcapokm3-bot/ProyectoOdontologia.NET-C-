namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;

public interface IRepositorioOdontologo
{

    Task<IEnumerable<Odontologo>> ObtenerTodos();

    Task<Odontologo?> ObtenerOdontologoPorId(int id);

    Task<List<Odontologo>> BuscarOdontologos(string criterio);


    Task AgregarOdontologo(Odontologo odontologo);

    Task ActualizarOdontologo(Odontologo odontologo);

    Task<bool> EliminarOdontologo(int id);
}