
namespace CapaDominio.Entidades;

public class Insumo
{
    public int Id { get; init; }
    public string Nombre { get; set; }
    public int Stock { get; set; }
    public int PuntoPedido { get; set; }

    public Insumo(int id, string nombre, int stock, int puntoPedido)
    {
        if (id <= 0) { throw new ArgumentException("El ID debe ser mayor a cero."); }
        if (string.IsNullOrWhiteSpace(nombre)) { throw new ArgumentException("El nombre es obligatorio."); }
        if (stock < 0) { throw new ArgumentException("El stock no puede ser negativo."); }
        if (puntoPedido < 0) { throw new ArgumentException("El punto de pedido no puede ser negativo."); }

        Id = id;
        Nombre = nombre;
        Stock = stock;
        PuntoPedido = puntoPedido;
    }
}