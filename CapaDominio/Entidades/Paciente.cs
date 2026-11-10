
namespace CapaDominio.Entidades;


public class Paciente
{
    public int Id { get; private set; }
    public int? UsuarioId { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public DateTime FechaNacimiento { get; private set; }
    public string Telefono { get; private set; }
    public string Email { get; private set; }

    public Paciente(int id, string nombre, string apellido, DateTime fechaNacimiento, string telefono, string email, int? usuarioId = null)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser un número positivo.", nameof(id));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
        if (string.IsNullOrWhiteSpace(apellido)) throw new ArgumentException("El apellido no puede estar vacío.", nameof(apellido));
        if (fechaNacimiento > DateTime.Now) throw new ArgumentException("La fecha de nacimiento no puede ser futura.", nameof(fechaNacimiento));
        
        DateTime fechaLimiteMaxima = new DateTime(2007, 12, 31);
        DateTime fechaLimiteMinima = new DateTime(1920, 1, 1);

        if (fechaNacimiento > fechaLimiteMaxima || fechaNacimiento < fechaLimiteMinima)
            throw new ArgumentException("El paciente debe tener una fecha válida.", nameof(fechaNacimiento));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.", nameof(email));
        if (string.IsNullOrWhiteSpace(telefono)) throw new ArgumentException("El teléfono es obligatorio.", nameof(telefono));

        Id = id;
        UsuarioId = usuarioId;
        Nombre = nombre;
        Apellido = apellido;
        FechaNacimiento = fechaNacimiento;
        Telefono = telefono;
        Email = email;
    }
}
