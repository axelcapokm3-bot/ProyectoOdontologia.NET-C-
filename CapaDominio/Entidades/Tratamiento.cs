
namespace CapaDominio.Entidades;

public class Tratamiento
{
    public int Id { get; init; }
    public string Descripcion { get; set; }
    public decimal Costo { get; set; }

    public Tratamiento(int id, string descripcion, decimal costo)
    {
        if (id <= 0)
            throw new ArgumentException("El ID debe ser un número positivo.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción no puede estar vacía.");

        if (costo < 0)
            throw new ArgumentException("El costo no puede ser negativo.");

        Id = id;
        Descripcion = descripcion;
        Costo = costo;
    }
}
