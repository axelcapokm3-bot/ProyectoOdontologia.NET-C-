namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;

public interface IRepositorioTurnos
{
    // Obtiene todos los turnos de la lista en memoria
    List<Turnos> ObtenerTodos();

    // Busca un turno por su ID
    Turnos? ObtenerTurnoPorId(int id);

    // Añade un nuevo turno a la memoria
    void AgregarTurno(Turnos turno);

    // Actualiza los datos de un turno existente
    void ActualizarTurno(Turnos turno);

    // Elimina un turno de la memoria usando su ID
    void EliminarTurno(int id);
}