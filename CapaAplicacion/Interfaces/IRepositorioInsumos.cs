namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;


public interface IRepositorioInsumos
{

    Task<IEnumerable<Insumo>> ObtenerTodos();


    Task<Insumo?> ObtenerInsumoPorId(int id);

    Task<List<Insumo>> BuscarInsumos(string criterio);


    Task AgregarInsumo(Insumo insumo);


    Task ActualizarInsumo(Insumo insumo);

    Task<bool> EliminarInsumo(int id);
}