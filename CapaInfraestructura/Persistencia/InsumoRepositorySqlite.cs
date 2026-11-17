namespace CapaInfraestructura.Persistencia;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class InsumoRepositorySqlite : IRepositorioInsumos
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private readonly IUnitOfWork _unitOfWork;

    public InsumoRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Insumo>> ObtenerTodos()
    {
        const string sql = @"
SELECT Id, Nombre, Categoria, Stock, StockReservado, StockMinimo, PuntoPedido, StockSeguridad, StockMaximo
FROM Insumos
ORDER BY Nombre;";

        IEnumerable<InsumoRow> resultado = await _unitOfWork.Connection.QueryAsync<InsumoRow>(
            new CommandDefinition(sql, transaction: _unitOfWork.Transaction));

        List<Insumo> insumos = new List<Insumo>();

        foreach (InsumoRow fila in resultado)
        {
            insumos.Add(Convertir(fila));
        }

        return insumos;
    }

    public async Task<Insumo?> ObtenerInsumoPorId(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "El ID debe ser mayor a cero.");

        const string sql = @"
SELECT Id, Nombre, Categoria, Stock, StockReservado, StockMinimo, PuntoPedido, StockSeguridad, StockMaximo
FROM Insumos
WHERE Id = @Id;";

        InsumoRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<InsumoRow>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        if (fila == null)
            return null;

        return Convertir(fila);
    }

    public async Task<List<Insumo>> BuscarInsumos(string criterio)
    {
        string busqueda = criterio?.Trim() ?? string.Empty;

        if (busqueda.Length == 0)
        {
            IEnumerable<Insumo> todos = await ObtenerTodos();
            return new List<Insumo>(todos);
        }

        const string sql = @"
SELECT Id, Nombre, Categoria, Stock, StockReservado, StockMinimo, PuntoPedido, StockSeguridad, StockMaximo
FROM Insumos
WHERE CAST(Id AS TEXT) = @Texto
   OR Nombre LIKE @Patron
   OR CASE Categoria
          WHEN 1 THEN 'Descartable'
          WHEN 2 THEN 'Anestesia'
          WHEN 3 THEN 'Restauracion'
          WHEN 4 THEN 'Instrumental'
          ELSE ''
      END LIKE @Patron
ORDER BY Nombre;";

        IEnumerable<InsumoRow> resultado = await _unitOfWork.Connection.QueryAsync<InsumoRow>(
            new CommandDefinition(
                sql,
                new
                {
                    Texto = busqueda,
                    Patron = "%" + busqueda + "%"
                },
                _unitOfWork.Transaction));

        List<Insumo> insumos = new List<Insumo>();

        foreach (InsumoRow fila in resultado)
        {
            insumos.Add(Convertir(fila));
        }

        return insumos;
    }

    public async Task AgregarInsumo(Insumo insumo)
    {
        if (insumo == null)
            throw new ArgumentNullException(nameof(insumo));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            const string sql = @"
INSERT INTO Insumos (Id, Nombre, Categoria, Stock, StockReservado, StockMinimo, PuntoPedido, StockSeguridad, StockMaximo)
VALUES (@Id, @Nombre, @Categoria, @Stock, @StockReservado, @StockMinimo, @PuntoPedido, @StockSeguridad, @StockMaximo);";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(insumo),
                    _unitOfWork.Transaction));

            if (filas != 1)
                throw new InvalidOperationException($"No se pudo insertar el insumo con ID {insumo.Id}.");

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task ActualizarInsumo(Insumo insumo)
    {
        if (insumo == null)
            throw new ArgumentNullException(nameof(insumo));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;

        if (transaccionPropia)
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            int? stockAnterior = await ObtenerStockAsync(insumo.Id);

            if (!stockAnterior.HasValue)
                throw new KeyNotFoundException($"No se encontró un insumo con el ID {insumo.Id}.");

            const string sql = @"
UPDATE Insumos
SET Nombre = @Nombre,
    Categoria = @Categoria,
    Stock = @Stock,
    StockReservado = @StockReservado,
    StockMinimo = @StockMinimo,
    PuntoPedido = @PuntoPedido,
    StockSeguridad = @StockSeguridad,
    StockMaximo = @StockMaximo
WHERE Id = @Id;";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    CrearParametros(insumo),
                    _unitOfWork.Transaction));

            if (filas == 0)
                throw new KeyNotFoundException($"No se encontró un insumo con el ID {insumo.Id}.");

            if (stockAnterior.Value != insumo.Stock)
                await RegistrarMovimientoAsync(insumo.Id, stockAnterior.Value, insumo.Stock);

            if (transaccionPropia)
                await _unitOfWork.CommitAsync();
        }
        catch
        {
            if (transaccionPropia)
                await _unitOfWork.RollbackAsync();

            throw;
        }
    }

    public async Task<bool> EliminarInsumo(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "El ID debe ser mayor a cero.");

        const string sql = @"
DELETE FROM Insumos
WHERE Id = @Id;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _unitOfWork.Transaction));

        return filas > 0;
    }

    public int IdAutoIncrementoSincrono()
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Insumos;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    private async Task<int?> ObtenerStockAsync(int id)
    {
        const string sql = @"
SELECT Stock
FROM Insumos
WHERE Id = @Id;";

        InsumoStockRow? fila = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<InsumoStockRow>(
            new CommandDefinition(
                sql,
                new { Id = id },
                _unitOfWork.Transaction));

        if (fila == null)
            return null;

        return checked((int)fila.Stock);
    }

    private async Task RegistrarMovimientoAsync(int insumoId, int stockAnterior, int stockNuevo)
    {
        TipoMovimientoStock tipoMovimiento = TipoMovimientoStock.Ajuste;
        int cantidad = Math.Abs(stockNuevo - stockAnterior);

        if (stockNuevo > stockAnterior)
        {
            tipoMovimiento = TipoMovimientoStock.Entrada;
        }
        else if (stockNuevo < stockAnterior)
        {
            tipoMovimiento = TipoMovimientoStock.Salida;
        }

        MovimientoStock movimiento = new MovimientoStock(
            insumoId,
            tipoMovimiento,
            cantidad,
            stockAnterior,
            stockNuevo,
            DateTime.Now,
            "Actualización de stock");

        const string sql = @"
INSERT INTO MovimientosStock (InsumoId, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, FechaHora, Motivo)
VALUES (@InsumoId, @TipoMovimiento, @Cantidad, @StockAnterior, @StockNuevo, @FechaHora, @Motivo);";

        await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    InsumoId = movimiento.InsumoId,
                    TipoMovimiento = (int)movimiento.TipoMovimiento,
                    Cantidad = movimiento.Cantidad,
                    StockAnterior = movimiento.StockAnterior,
                    StockNuevo = movimiento.StockNuevo,
                    FechaHora = movimiento.FechaHora.ToString(FormatoFecha, CultureInfo.InvariantCulture),
                    Motivo = movimiento.Motivo
                },
                _unitOfWork.Transaction));
    }

    private static Insumo Convertir(InsumoRow fila)
    {
        int id = checked((int)fila.Id);
        int stock = checked((int)fila.Stock);
        int stockReservado = checked((int)fila.StockReservado);
        int stockMinimo = checked((int)fila.StockMinimo);
        int puntoPedido = checked((int)fila.PuntoPedido);
        int stockSeguridad = checked((int)fila.StockSeguridad);
        int stockMaximo = checked((int)fila.StockMaximo);
        CategoriaInsumo categoria = (CategoriaInsumo)checked((int)fila.Categoria);

        return new Insumo(
            id,
            fila.Nombre,
            categoria,
            stock,
            stockReservado,
            puntoPedido,
            stockSeguridad,
            stockMinimo,
            stockMaximo);
    }

    private static object CrearParametros(Insumo insumo)
    {
        return new
        {
            Id = insumo.Id,
            Nombre = insumo.Nombre,
            Categoria = (int)insumo.Categoria,
            Stock = insumo.Stock,
            StockReservado = insumo.StockReservado,
            StockMinimo = insumo.StockMinimo,
            PuntoPedido = insumo.PuntoPedido,
            StockSeguridad = insumo.StockSeguridad,
            StockMaximo = insumo.StockMaximo
        };
    }
}

internal sealed class InsumoRow
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public long Categoria { get; set; }
    public long Stock { get; set; }
    public long StockReservado { get; set; }
    public long StockMinimo { get; set; }
    public long PuntoPedido { get; set; }
    public long StockSeguridad { get; set; }
    public long StockMaximo { get; set; }
}

internal sealed class InsumoStockRow
{
    public long Stock { get; set; }
}
