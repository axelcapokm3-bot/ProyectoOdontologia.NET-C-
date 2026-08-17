namespace CapaDominio.Entidades;

public class Tratamiento
{
    private List<TratamientoInsumo> insumosRequeridos = new();

    public int Id { get; init; }
    public string Descripcion { get; set; }
    public decimal Costo { get; set; }

    public List<TratamientoInsumo> InsumosRequeridos
    {
        get => insumosRequeridos;
        set => insumosRequeridos = value;
    }


    public Tratamiento(int id, string descripcion, decimal costo, List<TratamientoInsumo> insumosRequeridos)
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


        InsumosRequeridos = insumosRequeridos ?? new List<TratamientoInsumo>();
    }

    public void DescontarInsumos()
    {
        foreach (var detalle in InsumosRequeridos)
        {
            if (detalle.Insumo != null)
            {

                detalle.Insumo.ReducirStock(detalle.CantidadUsada);
            }
        }
    }



}
