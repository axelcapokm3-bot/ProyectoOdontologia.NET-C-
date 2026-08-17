namespace CapaDominio.Entidades;

public class TratamientoInsumo
{
    public int TratamientoId { get; set; }
    public int InsumoId { get; set; }
    public int CantidadUsada { get; set; }


    public Insumo Insumo { get; set; } = null!;


    public TratamientoInsumo() { }


    public TratamientoInsumo(int tratamientoId, int insumoId, int cantidadUsada)
    {
        if (cantidadUsada <= 0){
            throw new ArgumentException("La cantidad usada debe ser mayor a cero.");
        }

        if(tratamientoId == null || tratamientoId < 0 )
        {
             throw new ArgumentException(" No debe ser nulo ni debe ser negativo");
        }

          if(insumoId == null || insumoId < 0 )
        {
             throw new ArgumentException(" No debe ser nulo ni debe ser negativo");
        }


        TratamientoId = tratamientoId;
        InsumoId = insumoId;
        CantidadUsada = cantidadUsada;
    }
}