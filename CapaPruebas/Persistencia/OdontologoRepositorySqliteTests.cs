namespace ProyectoOdontologia.Tests.Persistencia;

using CapaDominio.Entidades;
using CapaInfraestructura.Persistencia;

public class OdontologoRepositorySqliteTests
{
    private static Odontologo CrearOdontologo(int id, string nombre, string matricula)
    {
        return new Odontologo(
            id,
            nombre,
            matricula,
            "Odontología general",
            "12345678",
            5);
    }

    [Fact]
    public async Task AgregarYRecuperar_PreservaTodosLosDatos()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        OdontologoRepositorySqlite repositorio = new OdontologoRepositorySqlite(unitOfWork);
        Odontologo odontologo = CrearOdontologo(1, "Carlos", "MAT-001");

        // Act
        await repositorio.AgregarOdontologo(odontologo);
        Odontologo? recuperado = await repositorio.ObtenerOdontologoPorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal(1, recuperado!.Id);
        Assert.Equal(5, recuperado.UsuarioId);
        Assert.Equal("Carlos", recuperado.Nombre);
        Assert.Equal("MAT-001", recuperado.Matricula);
        Assert.Equal("Odontología general", recuperado.Especialidad);
        Assert.Equal("12345678", recuperado.Telefono);
    }

    [Fact]
    public async Task BuscarOdontologos_CriterioParcial_RetornaCoincidencias()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        OdontologoRepositorySqlite repositorio = new OdontologoRepositorySqlite(unitOfWork);

        await repositorio.AgregarOdontologo(CrearOdontologo(1, "Carlos", "MAT-001"));
        await repositorio.AgregarOdontologo(CrearOdontologo(2, "Laura", "MAT-002"));

        // Act
        List<Odontologo> resultados = await repositorio.BuscarOdontologos("MAT-002");

        // Assert
        Odontologo unico = Assert.Single(resultados);
        Assert.Equal(2, unico.Id);
    }

    [Fact]
    public async Task ActualizarYEliminar_ReflejaLosCambios()
    {
        // Arrange
        await using SqliteTestDatabase baseDeDatos = new SqliteTestDatabase();
        await baseDeDatos.InicializarAsync();
        await using UnitOfWorkSqlite unitOfWork = baseDeDatos.CrearUnitOfWork();
        OdontologoRepositorySqlite repositorio = new OdontologoRepositorySqlite(unitOfWork);
        await repositorio.AgregarOdontologo(CrearOdontologo(1, "Carlos", "MAT-001"));

        Odontologo actualizado = new Odontologo(
            1,
            "Carlos",
            "MAT-001",
            "Ortodoncia",
            "87654321",
            5);

        // Act
        await repositorio.ActualizarOdontologo(actualizado);
        Odontologo? recuperado = await repositorio.ObtenerOdontologoPorId(1);
        bool eliminado = await repositorio.EliminarOdontologo(1);
        Odontologo? despuesDeEliminar = await repositorio.ObtenerOdontologoPorId(1);

        // Assert
        Assert.NotNull(recuperado);
        Assert.Equal("Ortodoncia", recuperado!.Especialidad);
        Assert.Equal("87654321", recuperado.Telefono);
        Assert.True(eliminado);
        Assert.Null(despuesDeEliminar);
    }
}
