using CapaAplicacion.Interfaces;
using ProyectoOdontologia.CapaAplicacion.ServiciosAplicacion;
using CapaInfraestructura.Implementacion;
using TuProyecto.WebAPI.Middlewares;
using CapaWebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepositorioPaciente, RepositorioPaciente>();
builder.Services.AddSingleton<IRepositorioOdontologo, RepositorioOdontologo>();
builder.Services.AddSingleton<IRepositorioInsumos, RepositorioInsumos>();
builder.Services.AddSingleton<IRepositorioTratamiento, RepositorioTratamiento>();
builder.Services.AddSingleton<IRepositorioTurnos, RepositorioTurnos>();


builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOdontologoService, OdontologoService>();
builder.Services.AddScoped<IInsumosService, InsumosService>();
builder.Services.AddScoped<ITratamientoService, TratamientoService>();
builder.Services.AddScoped<ITurnosService, TurnosService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
//app.UseDeveloperExceptionPage(); FUERZA A A LA API A CAPTURAR CUALQUIERO EXCEPCION Y MOSTRAR EN EL BACKEND 
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("PermitirTodo");
app.UseAuthorization();
app.UseExceptionHandler();

app.MapControllers();
app.Run();