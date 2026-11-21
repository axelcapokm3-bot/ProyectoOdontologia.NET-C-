namespace ProyectoOdontologia.Tests;

using CapaInfraestructura.Persistencia;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using ProyectoOdontologia.CapaAplicacion.DtosInput;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

[Trait("Category", "PacienteAutorizacion")]
public class PacienteAutorizacionTests : IClassFixture<PacienteAutorizacionTests.ApiFactory>
{
    private readonly HttpClient _clienteAnonimo;
    private readonly ApiFactory _fabrica;

    public PacienteAutorizacionTests(ApiFactory fabrica)
    {
        _fabrica = fabrica;
        _clienteAnonimo = fabrica.CreateClient();
        _fabrica.SembrarDatosInicialesAsync().GetAwaiter().GetResult();
    }

    private static StringContent Json(object contenido)
    {
        string json = JsonSerializer.Serialize(contenido);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static int _contadorTelefono = 555000000;

    private static async Task<AuthToken> RegistrarEPaciente(HttpClient cliente, string sufijo)
    {
        string nombreUsuario = "paciente_" + sufijo;
        string email = sufijo + "@example.com";
        int telefonoNumero = System.Threading.Interlocked.Increment(ref _contadorTelefono);

        RegistroPacienteRequestDto registroDto = new RegistroPacienteRequestDto
        {
            NombreUsuario = nombreUsuario,
            Email = email,
            Password = "ClaveSegura123!",
            Nombre = "Paciente",
            Apellido = "Test " + sufijo,
            FechaNacimiento = new DateTime(1990, 1, 1),
            Telefono = telefonoNumero.ToString()
        };

        HttpResponseMessage respuestaRegistro = await cliente.PostAsync("/api/Auth/registro", Json(registroDto));

        if (respuestaRegistro.StatusCode == HttpStatusCode.BadRequest)
        {
            return await ObtenerTokenLogin(cliente, nombreUsuario, "ClaveSegura123!");
        }

        Assert.Equal(HttpStatusCode.Created, respuestaRegistro.StatusCode);

        string cuerpoRegistro = await respuestaRegistro.Content.ReadAsStringAsync();
        using JsonDocument docRegistro = JsonDocument.Parse(cuerpoRegistro);
        string token = docRegistro.RootElement.GetProperty("accessToken").GetString()!;
        int usuarioId = docRegistro.RootElement.GetProperty("usuarioId").GetInt32();

        return new AuthToken(token, usuarioId);
    }

    private static async Task<AuthToken> ObtenerTokenLogin(HttpClient cliente, string nombreUsuario, string password)
    {
        LoginRequestDto loginDto = new LoginRequestDto
        {
            NombreUsuario = nombreUsuario,
            Password = password
        };

        HttpResponseMessage respuestaLogin = await cliente.PostAsync("/api/Auth/login", Json(loginDto));
        Assert.Equal(HttpStatusCode.OK, respuestaLogin.StatusCode);

        string cuerpoLogin = await respuestaLogin.Content.ReadAsStringAsync();
        using JsonDocument docLogin = JsonDocument.Parse(cuerpoLogin);
        string token = docLogin.RootElement.GetProperty("accessToken").GetString()!;
        int usuarioId = docLogin.RootElement.GetProperty("usuarioId").GetInt32();

        return new AuthToken(token, usuarioId);
    }

    private static HttpClient ClienteConToken(ApiFactory fabrica, string token)
    {
        HttpClient cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static async Task<int> CrearTurnoComoPaciente(HttpClient cliente, int odontologoId, int tratamientoId, DateTime inicio)
    {
        CrearTurnoRequestDto turnoDto = new CrearTurnoRequestDto
        {
            FechaHoraInicio = inicio,
            PacienteId = 999,
            OdontologoId = odontologoId,
            TratamientoId = tratamientoId
        };

        HttpResponseMessage respuesta = await cliente.PostAsync("/api/Turnos", Json(turnoDto));
        Assert.True(respuesta.StatusCode == HttpStatusCode.Created, "Esperaba 201 Created, obtuvo " + respuesta.StatusCode);

        string cuerpo = await respuesta.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(cuerpo);
        return doc.RootElement.GetInt32();
    }

    [Fact]
    public async Task Paciente_PuedeListarOdontologos()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "odontologos");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Odontologo");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_PuedeListarTratamientos()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "tratamientos");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Tratamiento");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_PuedeRegistrarTurnoPropio()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "registrarturno");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);
        DateTime inicio = new DateTime(2026, 12, 1, 9, 0, 0);

        int turnoId = await CrearTurnoComoPaciente(cliente, 1, 1, inicio);

        Assert.True(turnoId > 0);
    }

    [Fact]
    public async Task Paciente_ListarMisTurnosRetornaSoloLosPropios()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "misturnos");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);
        DateTime inicio = new DateTime(2026, 12, 2, 10, 0, 0);
        await CrearTurnoComoPaciente(cliente, 1, 1, inicio);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Turnos/mis-turnos");
        string cuerpo = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using JsonDocument doc = JsonDocument.Parse(cuerpo);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array);
        int cantidad = doc.RootElement.GetArrayLength();
        Assert.True(cantidad >= 1);
    }

    [Fact]
    public async Task Paciente_PuedeCancelarTurnoPropio()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "cancelar");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);
        DateTime inicio = new DateTime(2026, 12, 3, 11, 0, 0);
        int turnoId = await CrearTurnoComoPaciente(cliente, 1, 1, inicio);

        HttpResponseMessage respuesta = await cliente.PatchAsync("/api/Turnos/" + turnoId + "/cancelar", null);

        Assert.True(respuesta.StatusCode == HttpStatusCode.NoContent || respuesta.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Paciente_NoPuedeListarTodosLosTurnos()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "noturnos");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Turnos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeListarPacientes()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "nopacientes");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Paciente");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeCrearPacientes()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "nocrearpaciente");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        PacienteDtoInput nuevoPaciente = new PacienteDtoInput
        {
            Nombre = "Otro",
            Apellido = "Paciente",
            Email = "otro@example.com",
            FechaNacimiento = new DateTime(1995, 5, 5),
            Telefono = "5559999"
        };

        HttpResponseMessage respuesta = await cliente.PostAsync("/api/Paciente", Json(nuevoPaciente));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeListarInsumos()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "noinsumos");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Insumos");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeListarUsuarios()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "nousuarios");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeCompletarTurno()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "nocompletar");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);
        DateTime inicio = new DateTime(2026, 12, 4, 12, 0, 0);
        int turnoId = await CrearTurnoComoPaciente(cliente, 1, 1, inicio);

        HttpResponseMessage respuesta = await cliente.PatchAsync("/api/Turnos/" + turnoId + "/completar", null);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_NoPuedeCancelarTurnoDeOtroPaciente()
    {
        AuthToken tokenA = await RegistrarEPaciente(_clienteAnonimo, "ida");
        AuthToken tokenB = await RegistrarEPaciente(_clienteAnonimo, "idb");

        HttpClient clienteA = ClienteConToken(_fabrica, tokenA.Token);
        HttpClient clienteB = ClienteConToken(_fabrica, tokenB.Token);

        DateTime inicio = new DateTime(2026, 12, 5, 13, 0, 0);
        int turnoIdA = await CrearTurnoComoPaciente(clienteA, 1, 1, inicio);

        HttpResponseMessage respuesta = await clienteB.PatchAsync("/api/Turnos/" + turnoIdA + "/cancelar", null);

        Assert.True(respuesta.StatusCode == HttpStatusCode.Unauthorized || respuesta.StatusCode == HttpStatusCode.Forbidden || respuesta.StatusCode == HttpStatusCode.Conflict,
            "Esperaba 401 Unauthorized, 403 Forbidden o 409 Conflict, obtuvo " + respuesta.StatusCode);
    }

    [Fact]
    public async Task Paciente_CrearTurnoUsaSuPropioPacienteId()
    {
        AuthToken token = await RegistrarEPaciente(_clienteAnonimo, "propioid");
        HttpClient cliente = ClienteConToken(_fabrica, token.Token);
        DateTime inicio = new DateTime(2026, 12, 6, 14, 0, 0);

        int turnoId = await CrearTurnoComoPaciente(cliente, 1, 1, inicio);

        Assert.True(turnoId > 0);

        HttpResponseMessage respuesta = await cliente.GetAsync("/api/Turnos/mis-turnos");
        string cuerpo = await respuesta.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(cuerpo);
        bool encontrado = false;
        foreach (JsonElement turno in doc.RootElement.EnumerateArray())
        {
            if (turno.TryGetProperty("id", out JsonElement idProp) && idProp.GetInt32() == turnoId)
            {
                encontrado = true;
                break;
            }
        }
        Assert.True(encontrado, "El turno creado no aparece en mis-turnos del paciente autenticado.");
    }

    public sealed class AuthToken
    {
        public string Token { get; }
        public int UsuarioId { get; }

        public AuthToken(string token, int usuarioId)
        {
            Token = token;
            UsuarioId = usuarioId;
        }
    }

    public sealed class PacienteDtoInput
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
    }

    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _rutaArchivo;
        private static bool _datosSembrados = false;
        private static readonly object _lock = new object();

        public string ConnectionString { get; }
        public string RutaArchivo => _rutaArchivo;

        public ApiFactory()
        {
            _rutaArchivo = Path.Combine(Path.GetTempPath(), $"odontologia-authtest-{Guid.NewGuid():N}.db");
            ConnectionString = "Data Source=" + _rutaArchivo + ";Foreign Keys=True";
            System.Environment.SetEnvironmentVariable("ConnectionStrings__ClinicaDb", ConnectionString);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((contexto, configuracion) =>
            {
                configuracion.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Issuer"] = "OdontoClinicaApi",
                    ["Jwt:Audience"] = "OdontoClinicaClient",
                    ["Jwt:Key"] = "CLAVE_DE_PRUEBA_SOBRE_32_CARACTERES_PARA_HMACSHA256",
                    ["Jwt:ExpireHours"] = "8",
                    ["Seed:AdminPassword"] = string.Empty
                });
            });
        }

        public async Task SembrarDatosInicialesAsync()
        {
            lock (_lock)
            {
                if (_datosSembrados)
                {
                    return;
                }
            }

            SqliteConnectionFactory factory = new SqliteConnectionFactory(ConnectionString);
            await DatabaseInitializer.InicializarAsync(factory);

            using IDbConnection connection = factory.CreateConnection();
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                connection.Execute(
                    "INSERT INTO Odontologos (Nombre, Matricula, Especialidad, Telefono) VALUES (@nombre, @matricula, @especialidad, @telefono)",
                    new { nombre = "Odontólogo Test", matricula = "MAT-TEST01", especialidad = "Odontología General", telefono = "555000001" },
                    transaction);

                connection.Execute(
                    "INSERT INTO Tratamientos (Descripcion, Costo, DuracionEstimadaTicks) VALUES (@descripcion, @costo, @ticks)",
                    new { descripcion = "Limpieza Dental", costo = "5000", ticks = TimeSpan.FromMinutes(45).Ticks },
                    transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            lock (_lock)
            {
                _datosSembrados = true;
            }
        }
    }
}
