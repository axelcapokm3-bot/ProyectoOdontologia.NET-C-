namespace CapaAplicacion.Interfaces;

using CapaDominio.Entidades;


public interface IRepositorioInsumos
{
    // Obtiene todos los insumos de la lista en memoria
    public List<Insumo> ObtenerTodos();

    // Busca un insumo por su ID
    Insumo? ObtenerInsumoPorId(int id);

    // Añade un nuevo insumo a la memoria
    void AgregarInsumo(Insumo insumo);

    // Actualiza los datos de un insumo existente
    void ActualizarInsumo(Insumo insumo);

    // Elimina un insumo de la memoria usando su ID
    void EliminarInsumo(int id);
}