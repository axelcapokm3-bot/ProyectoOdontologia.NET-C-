using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using CapaInfraestructura.Implementacion;
using CapaAplicacion.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// 1. Servicios del Sistema
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOdontologoService, OdontologoService>();
builder.Services.AddScoped<ITurnosService, TurnosService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<IInsumosService, InsumosService>();

builder.Services.AddScoped<IRepositorioPaciente, RepositorioPaciente>();
builder.Services.AddScoped<IPacienteService, PacienteService>();


builder.Services.AddScoped<IRepositorioPaciente, RepositorioPaciente>();
builder.Services.AddScoped<IRepositorioOdontologo, RepositorioOdontologo>();
builder.Services.AddScoped<IRepositorioTurnos, RepositorioTurnos>();
builder.Services.AddScoped<IRepositorioTratamiento, RepositorioTratamiento>();
builder.Services.AddScoped<IRepositorioInsumos, RepositorioInsumos>();

// 3. Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 4. Middlewares de Entorno de Desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. El ORDEN CRÍTICO de los Middlewares
app.UseRouting(); // 1° Enrutar la petición

app.UseCors("PermitirTodo"); // 2° Aplicar CORS (¡Justo aquí!)
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthorization(); // 3° Autorizar

app.MapControllers(); // 4° Mapear a los Controladores

app.Run();