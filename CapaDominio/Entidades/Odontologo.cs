
namespace CapaDominio.Entidades;

public class Odontologo
{
    public int Id { get; init; }
    public int? UsuarioId { get; private set; }
    public string Nombre { get; set; }
    public string Matricula { get; init; }
    public string Especialidad { get; set; }
    public int Telefono { get; set; }

    public Odontologo(int id, string nombre, string matricula, string especialidad, int telefono, int? usuarioId = null)
    {
        if (id <= 0  || id.Equals(null))
        {
            throw new ArgumentException("El ID debe ser mayor a cero.");
        }
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.");
        }
        if (string.IsNullOrWhiteSpace(matricula))
        {
            throw new ArgumentException("La matrícula es obligatoria.");
        }
        if (string.IsNullOrWhiteSpace(especialidad))
        {
            throw new ArgumentException("La especialidad es obligatoria.");
        }

        string telefonoTexto = telefono.ToString();

        if (telefonoTexto.Length < 8 || telefonoTexto.Length > 10)
        {
            throw new ArgumentException("El teléfono es obligatorio.");
        }

        Id = id;
        UsuarioId = usuarioId;
        Nombre = nombre;
        Matricula = matricula;
        Especialidad = especialidad;
        Telefono = telefono;
    }
}
