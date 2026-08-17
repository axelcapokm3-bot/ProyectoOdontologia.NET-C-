namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IRepositorioPaciente
{

  Task<IEnumerable<Paciente>> ObtenerTodos();

  //sobrecarga de metodo overloading
  Task<List<Paciente>> BuscarPacientes(string criterio);
  Task<Paciente?> ObtenerPacientePorId(int id);


  Task AgregarPaciente(Paciente paciente);


  Task ActualizarPaciente(Paciente paciente);


  Task<bool> EliminarPaciente(int id);
}