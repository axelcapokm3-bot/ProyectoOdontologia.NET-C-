using CapaAplicacion.Interfaces;
using CapaAplicacion.Seguridad.Interfaces;
using CapaAplicacion.Seguridad.Servicios;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using CapaInfraestructura.Implementacion;
using CapaInfraestructura.Servicios;
using TuProyecto.WebAPI.Middlewares;
using CapaWebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepositorioPaciente, RepositorioPaciente>();
builder.Services.AddSingleton<IRepositorioOdontologo, RepositorioOdontologo>();
builder.Services.AddSingleton<IRepositorioInsumos, RepositorioInsumos>();
builder.Services.AddSingleton<IRepositorioTratamiento, RepositorioTratamiento>();
builder.Services.AddSingleton<IRepositorioTurnos, RepositorioTurnos>();
builder.Services.AddSingleton<IRepositorioUsuario, RepositorioUsuario>();


builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOdontologoService, OdontologoService>();
builder.Services.AddScoped<IInsumosService, InsumosService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<ITurnosService, TurnosService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioAdminService, UsuarioAdminService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherBCrypt>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.FromMinutes(8),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("PermitirTodo");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();

app.MapControllers();
app.Run();
