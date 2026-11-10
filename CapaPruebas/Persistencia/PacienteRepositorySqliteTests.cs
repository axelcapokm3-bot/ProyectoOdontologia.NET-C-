namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;

public class PacienteRepositorySqliteTests
{
    private static Paciente CrearPaciente(int id, string nombre, string apellido, string email)
    {
        return new Paciente(
            id,
            nombre,
            apellido,
            new DateTime(1990, 1, 1),
            "555-0100",
            email,
            7);
    }

    [Fact]
    public async Task AgregarYRecuperar_PreservaTodosLosDatos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        PacienteRepositorySqlite repositorio = new PacienteRepositorySqlite(unitOfWork);
        Paciente paciente = CrearPaciente(1, "Ana", "Gómez", "ana@example.com");

        // Act
        await repositorio.AgregarPaciente(paciente);
        Paciente? recuperado = await repositorio.ObtenerPacientePorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(1, recuperado!.Id);
        Assert.Equal(7, recuperado.UsuarioId);
        Assert.Equal("Ana", recuperado.Nombre);
        Assert.Equal("Gómez", recuperado.Apellido);
        Assert.Equal(new DateTime(1990, 1, 1), recuperado.FechaNacimiento);
        Assert.Equal("555-0100", recuperado.Telefono);
        Assert.Equal("ana@example.com", recuperado.Email);
    }

    [Fact]
    public async Task BuscarPacientes_CriterioParcial_RetornaCoincidencias()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        PacienteRepositorySqlite repositorio = new PacienteRepositorySqlite(unitOfWork);

        Paciente primero = CrearPaciente(1, "Ana", "Gómez", "ana@example.com");
        Paciente segundo = new Paciente(
            2,
            "Luis",
            "Pérez",
            new DateTime(1991, 2, 2),
            "555-0200",
            "luis@example.com",
            8);

        await repositorio.AgregarPaciente(primero);
        await repositorio.AgregarPaciente(segundo);

        // Act
        List<Paciente> resultados = await repositorio.BuscarPacientes("Gómez");

        // Assert
        Paciente unico = Assert.Single(resultados);
        Assert.Equal(1, unico.Id);
    }

    [Fact]
    public async Task IdAutoIncrementoSincrono_RetornaElSiguienteId()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        PacienteRepositorySqlite repositorio = new PacienteRepositorySqlite(unitOfWork);
        await repositorio.AgregarPaciente(CrearPaciente(5, "Ana", "Gómez", "ana@example.com"));

        // Act
        int siguiente = repositorio.IdAutoIncrementoSincrono();

        // Assert
        Assert.Equal(6, siguiente);
    }

    [Fact]
    public async Task ActualizarYEliminar_ReflejaLosCambios()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        PacienteRepositorySqlite repositorio = new PacienteRepositorySqlite(unitOfWork);
        await repositorio.AgregarPaciente(CrearPaciente(1, "Ana", "Gómez", "ana@example.com"));

        Paciente actualizado = CrearPaciente(1, "Ana", "Gómez", "nueva@example.com");

        // Act
        await repositorio.ActualizarPaciente(actualizado);
        Paciente? recuperado = await repositorio.ObtenerPacientePorId(1);
        bool eliminado = await repositorio.EliminarPaciente(1);
        Paciente? despuesDeEliminar = await repositorio.ObtenerPacientePorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal("nueva@example.com", recuperado!.Email);
        Assert.True(eliminado);
        Assert.Null(despuesDeEliminar);
    }
}
