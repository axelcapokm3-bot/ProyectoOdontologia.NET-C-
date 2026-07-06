
namespace CapaDominio.Entidades;

public class Odontologo
{
    public int Id { get; set; }
    public string Nombre { get; set; }

    public string Matricula { get; set; }

    public string Especialidad { get; set; }

    public int Telefono { get; set; }



    public Odontologo(int id, string nombre, string matricula, string especialidad, int telefono)
    {




        this.Id = id;
        this.Nombre = nombre;
        this.Matricula = matricula;
        this.Especialidad = especialidad;
        this.Telefono = telefono;
        if (id <= 0 || id == null)
        {
            throw new ArgumentException("El ID debe ser un número positivo");

        }
        if (nombre == null || nombre.IsWhiteSpace())
        {
            throw new ArgumentException("El nombre  esta vacio ");
        }
        if (matricula == null || matricula.IsWhiteSpace())
        {
            throw new ArgumentException("La matricula  esta vacio ");
        }
        if (especialidad == null || especialidad.IsWhiteSpace())
        {
            throw new ArgumentException("La especialidad  esta vacio ");
        }
        if (telefono <= 0 || telefono == null)
        {
            throw new ArgumentException("El telefono debe ser un número positivo");

        }




    }
}