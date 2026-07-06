namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;

public interface IRepositorioOdontologo
{
    // Obtiene todos los odontólogos de la lista en memoria
    public List<Odontologo> ObtenerTodos();

    // Busca un odontólogo por su ID
    public Odontologo? ObtenerOdontologoPorId(int id);

    // Añade un nuevo odontólogo a la memoria
    public void AgregarOdontologo(Odontologo odontologo);

    // Actualiza los datos de un odontólogo existente
    public void ActualizarOdontologo(Odontologo odontologo);

    // Elimina un odontólogo de la memoria usando su ID
    public void EliminarOdontologo(int id);
}