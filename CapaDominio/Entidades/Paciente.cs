
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
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser un número positivo.");
        }
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre no puede estar vacío.");
        }
        if (string.IsNullOrWhiteSpace(apellido))
        {
            throw new ArgumentException("El apellido no puede estar vacío.");
        }
        if (fechaNacimiento > DateTime.Now)
        {
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.");
        }
        DateTime fechaLimiteMaxima = new DateTime(2007, 12, 31);
        DateTime fechaLimiteMinima = new DateTime(1920, 1, 1);


        if (fechaNacimiento > fechaLimiteMaxima || fechaNacimiento < fechaLimiteMinima)
        {
            throw new ArgumentException("El paciente debe haber nacido en 2007 o antes.");
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("El email es obligatorio.");
        }
        if (string.IsNullOrWhiteSpace(telefono))
        {
            throw new ArgumentException("El teléfono es obligatorio.");
        }

        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        FechaNacimiento = fechaNacimiento;
        Telefono = telefono;
        Email = email;
    }
}