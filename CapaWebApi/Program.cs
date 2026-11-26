
using CapaAplicacion.Interfaces;
using CapaAplicacion.Seguridad.Interfaces;
using CapaAplicacion.Seguridad.Servicios;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using CapaInfraestructura.Persistencia;
using CapaInfraestructura.Servicios;
using CapaWebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddScoped<IRepositorioUsuario, UsuarioRepositorySqlite>();

string connectionString =
    builder.Configuration.GetConnectionString("ClinicaDb")
    ?? "Data Source=clinica.db;Foreign Keys=True";

builder.Services.AddSingleton<IDbConnectionFactory>(
    new SqliteConnectionFactory(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWorkSqlite>();

builder.Services.AddScoped<IRepositorioInsumos, InsumoRepositorySqlite>();
builder.Services.AddScoped<IRepositorioTratamiento, TratamientoRepositorySqlite>();
builder.Services.AddScoped<IRepositorioTurnos, TurnoRepositorySqlite>();
builder.Services.AddScoped<IRepositorioPaciente, PacienteRepositorySqlite>();
builder.Services.AddScoped<IRepositorioOdontologo, OdontologoRepositorySqlite>();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOdontologoService, OdontologoService>();
builder.Services.AddScoped<IInsumosService, InsumosService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<ITurnosService, TurnosService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasherBCrypt>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// ======================================================
// CONTROLADORES Y SWAGGER
// ======================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingresá tu token JWT."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});



builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!)),

            ClockSkew = TimeSpan.FromMinutes(8),

            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();



builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddProblemDetails();



var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    IDbConnectionFactory connectionFactory =
        scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

    await DatabaseInitializer.InicializarAsync(
        connectionFactory,
        passwordAdmin: builder.Configuration["Seed:AdminPassword"]);
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseCors("PermitirTodo");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
