namespace CapaDominio.Entidades;

using CapaDominio.Excepciones;
using System;

public class Insumo
{
    private readonly object _stockLock = new object();

    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public CategoriaInsumo Categoria { get; private set; }
    public int Stock { get; private set; }
    public int StockReservado { get; private set; }
    public int StockMinimo { get; private set; }
    public int PuntoPedido { get; private set; }
    public int StockSeguridad { get; private set; }
    public int StockMaximo { get; private set; }

    public Insumo(int id, string nombre, CategoriaInsumo categoria, int stock)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (stock < 0)
        {
            throw new ArgumentException("El stock no puede ser negativo.", nameof(stock));
        }

        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        Stock = stock;
        StockReservado = 0;
        StockSeguridad = CalculoStockSeguridad(categoria);
        PuntoPedido = StockSeguridad * 2;
        StockMinimo = PuntoPedido;
        StockMaximo = PuntoPedido * 3;
    }

    public Insumo(
        int id,
        string nombre,
        CategoriaInsumo categoria,
        int stock,
        int stockReservado,
        int puntoPedido,
        int stockSeguridad,
        int stockMaximo)
        : this(id, nombre, categoria, stock, stockReservado, puntoPedido, stockSeguridad, puntoPedido, stockMaximo)
    {
    }

    public Insumo(
        int id,
        string nombre,
        CategoriaInsumo categoria,
        int stock,
        int stockReservado,
        int puntoPedido,
        int stockSeguridad,
        int stockMinimo,
        int stockMaximo)
    {
        ValidarDatosBase(id, nombre, stock);

        if (stockReservado < 0)
        {
            throw new ArgumentException("El stock reservado no puede ser negativo.", nameof(stockReservado));
        }

        if (stockSeguridad < 0)
        {
            throw new ArgumentException("El stock de seguridad no puede ser negativo.", nameof(stockSeguridad));
        }

        if (stockMinimo < 0)
        {
            throw new ArgumentException("El stock mínimo no puede ser negativo.", nameof(stockMinimo));
        }

        if (puntoPedido < stockSeguridad)
        {
            throw new ArgumentException("El punto de pedido no puede ser menor al stock de seguridad.", nameof(puntoPedido));
        }

        if (stockMaximo < puntoPedido)
        {
            throw new ArgumentException("El stock máximo no puede ser menor al punto de pedido.", nameof(stockMaximo));
        }

        if (stockMinimo > stockMaximo)
        {
            throw new ArgumentException("El stock mínimo no puede superar el stock máximo.", nameof(stockMinimo));
        }

        if (stockReservado > stock)
        {
            throw new ArgumentException("El stock reservado no puede superar el stock total.", nameof(stockReservado));
        }

        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        Stock = stock;
        StockReservado = stockReservado;
        PuntoPedido = puntoPedido;
        StockSeguridad = stockSeguridad;
        StockMinimo = stockMinimo;
        StockMaximo = stockMaximo;
    }

    public void Actualizar(
        string nombre,
        CategoriaInsumo categoria,
        int stock,
        int stockReservado)
    {
        Actualizar(nombre, categoria, stock, stockReservado, StockMinimo);
    }

    public void Actualizar(
        string nombre,
        CategoriaInsumo categoria,
        int stock,
        int stockReservado,
        int stockMinimo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (stock < 0)
        {
            throw new ArgumentException("El stock no puede ser negativo.", nameof(stock));
        }

        if (stockReservado < 0)
        {
            throw new ArgumentException("El stock reservado no puede ser negativo.", nameof(stockReservado));
        }

        if (stockReservado > stock)
        {
            throw new ArgumentException("El stock reservado no puede superar el stock total.", nameof(stockReservado));
        }

        if (stockMinimo < 0)
        {
            throw new ArgumentException("El stock mínimo no puede ser negativo.", nameof(stockMinimo));
        }

        int stockSeguridad = CalculoStockSeguridad(categoria);
        int puntoPedido = stockSeguridad * 2;
        int stockMaximo = puntoPedido * 3;

        if (stockMinimo > stockMaximo)
        {
            throw new ArgumentException("El stock mínimo no puede superar el stock máximo.", nameof(stockMinimo));
        }

        Nombre = nombre;
        Categoria = categoria;
        Stock = stock;
        StockReservado = stockReservado;
        StockSeguridad = stockSeguridad;
        PuntoPedido = puntoPedido;
        StockMinimo = stockMinimo;
        StockMaximo = stockMaximo;
    }

    public int StockDisponible()
    {
        lock (_stockLock)
        {
            return ObtenerStockDisponibleInterno();
        }
    }

    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad a descontar debe ser mayor a cero.", nameof(cantidad));
        }

        lock (_stockLock)
        {
            int disponible = ObtenerStockDisponibleInterno();

            if (disponible < cantidad)
            {
                throw new StockInsuficienteException(Id, cantidad, disponible);
            }

            Stock -= cantidad;
        }
    }

    public void ReponerStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad a reponer debe ser mayor a cero.", nameof(cantidad));
        }

        lock (_stockLock)
        {
            Stock += cantidad;
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

    public bool RequiereReabastecimiento()
    {
        return StockDisponible() <= StockMinimo;
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

    private static void ValidarDatosBase(int id, string nombre, int stock)
    {
        if (id <= 0)
        {
            throw new ArgumentException("El ID debe ser mayor a cero.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre es obligatorio.", nameof(nombre));
        }

        if (stock < 0)
        {
            throw new ArgumentException("El stock no puede ser negativo.", nameof(stock));
        }
    }

    private int ObtenerStockDisponibleInterno()
    {
        return Stock - StockReservado;
    }

    private int CalculoStockSeguridad(CategoriaInsumo categoria)
    {
        switch (categoria)
        {
            case CategoriaInsumo.Descartable:
                return 200;
            case CategoriaInsumo.Anestesia:
                return 50;
            case CategoriaInsumo.Restauracion:
                return 20;
            case CategoriaInsumo.Instrumental:
                return 10;
            default:
                return 30;
        }
    }
}
