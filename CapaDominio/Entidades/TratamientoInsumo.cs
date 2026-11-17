namespace CapaDominio.Entidades;

public class TratamientoInsumo
{
    public int TratamientoId { get; private set; }
    public int InsumoId { get; private set; }
    public int CantidadUsada { get; private set; }

    public Insumo Insumo { get; private set; } = null!;

    public TratamientoInsumo(int tratamientoId, int insumoId, int cantidadUsada)
    {
        if (cantidadUsada <= 0)
        {
            throw new ArgumentException("La cantidad usada debe ser mayor a cero.", nameof(cantidadUsada));
        }

        if (tratamientoId < 0)
        {
            throw new ArgumentException("El ID del tratamiento no puede ser negativo.", nameof(tratamientoId));
        }

        if (insumoId <= 0)
        {
            throw new ArgumentException("El ID del insumo debe ser mayor a cero.", nameof(insumoId));
        }

        TratamientoId = tratamientoId;
        InsumoId = insumoId;
        CantidadUsada = cantidadUsada;
    }

    public void AsignarInsumo(Insumo insumo)
    {
        Insumo = insumo ?? throw new ArgumentNullException(nameof(insumo));
    }
}
