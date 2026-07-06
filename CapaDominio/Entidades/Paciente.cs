
namespace CapaDominio.Entidades;


public class Paciente
{
    public int Id { get; init; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }

    public Paciente(int id, string nombre, string apellido, DateTime fechaNacimiento, string telefono, string email)
    {
        // Validaciones (Guard Clauses)
        if (id <= 0)
            throw new ArgumentException("El ID debe ser un número positivo.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido no puede estar vacío.");

        if (fechaNacimiento > DateTime.Now)
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.");

        if (fechaNacimiento < DateTime.Now.AddYears(-120))
            throw new ArgumentException("La edad ingresada supera el límite permitido.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(telefono))
        {
            throw new ArgumentException("El teléfono es obligatorio.");
        }

        // Asignación
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        FechaNacimiento = fechaNacimiento;
        Telefono = telefono;
        Email = email;




    }
}