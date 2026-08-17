namespace CapaDominio.Entidades;

using System;

public class Insumo
{
    private readonly object _stockLock = new object(); 

    public int Id { get; init; }
    public string Nombre { get; set; }
    public CategoriaInsumo Categoria { get; set; }
    public int Stock { get; private set; }
    public int StockReservado { get; private set; }
    public int PuntoPedido { get; set; }
    public int StockSeguridad { get; private set; }
    public int StockMaximo { get; private set; }

    public Insumo(int id, string nombre, CategoriaInsumo categoria, int stock)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.");
        }

        if (stock <= 0)
        {
            throw new ArgumentException("El stock no puede ser negativo.");
        }

        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        Stock = stock;

        StockSeguridad = CalculoStockSeguridad(categoria);
        PuntoPedido = StockSeguridad * 2;
        StockMaximo = PuntoPedido * 3;
    }

    public int StockDisponible()
    {
        lock (_stockLock)
        {
            return ObtenerStockDisponibleInterno();
        }
    }

    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad a reducir debe ser mayor a cero.");
        }

        lock (_stockLock)
        {
            if (ObtenerStockDisponibleInterno() < cantidad)
            {
                throw new InvalidOperationException("No se puede reducir: la cantidad supera el stock disponible.");
            }

            Stock -= cantidad;
        }
    }

    public void AumentarStock(int cantidadIngresa)
    {
        if (cantidadIngresa <= 0)
        {
            throw new ArgumentException("La cantidad a ingresar debe ser mayor a cero.");
        }

        lock (_stockLock)
        {
            Stock += cantidadIngresa;
        }
    }

    public NivelCriticidad ObtenerEstadoStock()
    {
        int disponible = StockDisponible();

        if (disponible <= StockSeguridad)
        {
            return NivelCriticidad.Critico;
        }

        if (disponible < PuntoPedido)
        {
            return NivelCriticidad.Medio;
        }

        return NivelCriticidad.Bajo;
    }

    public bool RequiereReposicion()
    {
        NivelCriticidad estado = ObtenerEstadoStock();
        return estado == NivelCriticidad.Critico || estado == NivelCriticidad.Medio;
    }

    public int SugerirCantidadAReponer()
    {
        if (!RequiereReposicion())
        {
            return 0;
        }

        int cantidadFaltante = StockMaximo - Stock;
        return cantidadFaltante > 0 ? cantidadFaltante : 0;
    }

    // Helper privado para evitar doble lock cuando ya se está dentro de la sección crítica
    private int ObtenerStockDisponibleInterno()
    {
        return Stock - StockReservado;
    }

    private int CalculoStockSeguridad(CategoriaInsumo categoria)
    {
        return categoria switch
        {
            CategoriaInsumo.Descartable => 200,
            CategoriaInsumo.Anestesia => 50,
            CategoriaInsumo.Restauracion => 20,
            CategoriaInsumo.Instrumental => 10,
            _ => 30
        };
    }
}