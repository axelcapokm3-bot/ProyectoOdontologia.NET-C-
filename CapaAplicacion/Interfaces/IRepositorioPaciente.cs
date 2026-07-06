namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;

public interface IRepositorioPaciente
{
    // Obtiene todos los pacientes de la lista en memoria
    public List<Paciente> ObtenerTodos();

    // Busca un paciente por su ID
   public  Paciente? ObtenerPacientePorId(int id);

    // Añade un nuevo paciente a la memoria
   public  void AgregarPaciente(Paciente paciente);

    // Actualiza los datos de un paciente existente
  public   void ActualizarPaciente(Paciente paciente);

    // Elimina un paciente de la memoria usando su ID
 public    void EliminarPaciente(int id);
}