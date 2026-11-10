
namespace CapaDominio.Entidades;

public class Odontologo
{
    public int Id { get; private set; }
    public int? UsuarioId { get; private set; }
    public string Nombre { get; private set; }
    public string Matricula { get; private set; }
    public string Especialidad { get; private set; }
    public int Telefono { get; private set; }

    public Odontologo(int id, string nombre, string matricula, string especialidad, int telefono, int? usuarioId = null)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        if (string.IsNullOrWhiteSpace(matricula)) throw new ArgumentException("La matrícula es obligatoria.", nameof(matricula));
        if (string.IsNullOrWhiteSpace(especialidad)) throw new ArgumentException("La especialidad es obligatoria.", nameof(especialidad));

        string telefonoTexto = telefono.ToString();
        if (telefonoTexto.Length < 8 || telefonoTexto.Length > 10)
            throw new ArgumentException("El teléfono debe tener entre 8 y 10 dígitos.", nameof(telefono));

        Id = id;
        UsuarioId = usuarioId;
        Nombre = nombre;
        Matricula = matricula;
        Especialidad = especialidad;
        Telefono = telefono;
    }
}
