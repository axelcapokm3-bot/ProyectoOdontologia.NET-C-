namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;

public interface IRepositorioTratamiento
{
    // Obtiene todos los tratamientos de la lista en memoria
    List<Tratamiento> ObtenerTodos();

    // Busca un tratamiento por su ID
    Tratamiento? ObtenerTratamientoPorId(int id);

    // Añade un nuevo tratamiento a la memoria
    void AgregarTratamiento(Tratamiento tratamiento);

    // Actualiza los datos de un tratamiento existente
    void ActualizarTratamiento(Tratamiento tratamiento);

    // Elimina un tratamiento de la memoria usando su ID
    void EliminarTratamiento(int id);
}