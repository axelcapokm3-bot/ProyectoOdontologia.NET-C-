namespace ProyectoOdontologia.Tests.Helpers;

using CapaDominio.Entidades;

/// <summary>
/// Factoría centralizada de entidades de dominio válidas.
/// Se usa en Arrange para evitar repetir los parámetros obligatorios de
/// los constructores de dominio y garantizar que los datos cumplen las reglas
/// de negocio (IDs positivos, fechas dentro de rangos permitidos, etc.).
/// </summary>
public static class EntidadesFactory
{
    private const string TelefonoOdontologo = "35123456";

    public static Insumo CrearInsumo(int id = 1, string nombre = "Anestesia Lidocaina", int stock = 500, CategoriaInsumo categoria = CategoriaInsumo.Anestesia)
    {
        return new Insumo(id, nombre, categoria, stock);
    }

    public static Paciente CrearPaciente(int id = 1)
    {
        return new Paciente(
            id,
            "Juan",
            "Perez",
            new DateTime(1990, 5, 15),
            "3515551234",
            "juan.perez@email.com");
    }

    public static Odontologo CrearOdontologo(int id = 1)
    {
        return new Odontologo(
            id,
            "Dra. Maria Lopez",
            "MAT-1234",
            "Ortodoncia",
            TelefonoOdontologo);
    }

    public static Tratamiento CrearTratamiento(int id = 1)
    {
        return new Tratamiento(
            id,
            "Limpieza dental completa",
            5000m,
            new List<TratamientoInsumo>());
    }

    public static Turno CrearTurno(int id = 1)
    {
        DateTime inicio = DateTime.Now.AddDays(1);
        DateTime fin = inicio.AddHours(1);

        return new Turno(id, inicio, fin, pacienteId: 1, odontologoId: 1, tratamientoId: 1);
    }
}