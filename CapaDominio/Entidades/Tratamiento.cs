namespace CapaDominio.Entidades;

public class Tratamiento
{
    private List<TratamientoInsumo> insumosRequeridos = new();

    public int Id { get; private set; }
    public string Descripcion { get; private set; }
    public decimal Costo { get; private set; }
    public TimeSpan DuracionEstimada { get; private set; }
    public IReadOnlyCollection<TratamientoInsumo> InsumosRequeridos => insumosRequeridos.AsReadOnly();

    private Tratamiento()
    {
        Descripcion = null!;
        DuracionEstimada = TimeSpan.FromMinutes(30);
    }

    public Tratamiento(int id, string descripcion, decimal costo, List<TratamientoInsumo> insumosRequeridos)
        : this(id, descripcion, costo, insumosRequeridos, TimeSpan.FromMinutes(30))
    {
    }

    public Tratamiento(int id, string descripcion, decimal costo, TimeSpan duracionEstimada)
        : this(id, descripcion, costo, new List<TratamientoInsumo>(), duracionEstimada)
    {
    }

    public Tratamiento(int id, string descripcion, decimal costo, List<TratamientoInsumo> insumosRequeridos, TimeSpan duracionEstimada)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser positivo.", nameof(id));
        if (string.IsNullOrWhiteSpace(descripcion)) throw new ArgumentException("La descripción es obligatoria.", nameof(descripcion));
        if (costo < 0) throw new ArgumentException("El costo no puede ser negativo.", nameof(costo));
        if (duracionEstimada <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duracionEstimada), "La duración estimada debe ser mayor a cero.");

        Id = id;
        Descripcion = descripcion;
        Costo = costo;
        DuracionEstimada = duracionEstimada;
        this.insumosRequeridos = insumosRequeridos ?? new List<TratamientoInsumo>();
    }

    public void AgregarInsumo(TratamientoInsumo insumo)
    {
        if (insumo == null) throw new ArgumentNullException(nameof(insumo));
        insumosRequeridos.Add(insumo);
    }

}
