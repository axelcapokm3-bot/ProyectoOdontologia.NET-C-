namespace CapaInfraestructura.Persistencia;

using System.Globalization;
using CapaAplicacion.Interfaces;
using CapaDominio.Entidades;
using Dapper;

public sealed class TratamientoRepositorySqlite : IRepositorioTratamiento
{
    private readonly IUnitOfWork _unitOfWork;

    public TratamientoRepositorySqlite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Tratamiento>> ObtenerTodos()
    {
        const string sqlTratamientos = @"
SELECT Id, Descripcion, Costo, DuracionEstimadaTicks AS DuracionEstimada
FROM Tratamientos
ORDER BY Id;";

        const string sqlInsumos = @"
SELECT TratamientoId, InsumoId, CantidadUsada
FROM TratamientoInsumos
ORDER BY TratamientoId, InsumoId;";

        IEnumerable<Tratamiento> resultadoTratamientos = await _unitOfWork.Connection.QueryAsync<Tratamiento>(
            new CommandDefinition(sqlTratamientos, transaction: _unitOfWork.Transaction));

        IEnumerable<TratamientoInsumo> resultadoInsumos = await _unitOfWork.Connection.QueryAsync<TratamientoInsumo>(
            new CommandDefinition(sqlInsumos, transaction: _unitOfWork.Transaction));

        List<Tratamiento> tratamientos = new List<Tratamiento>();
        Dictionary<int, List<TratamientoInsumo>> insumosPorTratamiento = new Dictionary<int, List<TratamientoInsumo>>();

        foreach (Tratamiento tratamiento in resultadoTratamientos)
        {
            tratamientos.Add(tratamiento);
        }

        foreach (TratamientoInsumo insumo in resultadoInsumos)
        {
            if (!insumosPorTratamiento.ContainsKey(insumo.TratamientoId))
            {
                insumosPorTratamiento[insumo.TratamientoId] = new List<TratamientoInsumo>();
            }

            insumosPorTratamiento[insumo.TratamientoId].Add(insumo);
        }

        foreach (Tratamiento tratamiento in tratamientos)
        {
            if (insumosPorTratamiento.ContainsKey(tratamiento.Id))
            {
                AgregarInsumos(tratamiento, insumosPorTratamiento[tratamiento.Id]);
            }
        }

        return tratamientos;
    }

    public Task<Tratamiento?> ObtenerTratamientoPorId(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        return ObtenerTratamientoPorIdInternoAsync(id, CancellationToken.None);
    }

    public async Task<List<Tratamiento>> BuscarTratamientos(string criterio)
    {
        string busqueda = criterio?.Trim() ?? string.Empty;

        if (busqueda.Length == 0)
        {
            return (await ObtenerTodos()).ToList();
        }

        bool esNumerico = int.TryParse(busqueda, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idBuscado);

        if (esNumerico)
        {
            Tratamiento? porId = await ObtenerTratamientoPorIdInternoAsync(idBuscado, CancellationToken.None);
            if (porId is not null)
                return new List<Tratamiento> { porId };
        }

        const string sql = @"
SELECT Id, Descripcion, Costo, DuracionEstimadaTicks AS DuracionEstimada
FROM Tratamientos
WHERE CAST(Id AS TEXT) = @Texto
   OR Descripcion LIKE @Patron
ORDER BY Id;";

        const string sqlInsumos = @"
SELECT TratamientoId, InsumoId, CantidadUsada
FROM TratamientoInsumos
WHERE TratamientoId IN @Ids
ORDER BY TratamientoId, InsumoId;";

        IEnumerable<Tratamiento> resultadoTratamientos = await _unitOfWork.Connection.QueryAsync<Tratamiento>(
            new CommandDefinition(
                sql,
                new { Texto = busqueda, Patron = "%" + busqueda + "%" },
                _unitOfWork.Transaction));

        List<Tratamiento> tratamientos = new List<Tratamiento>();
        List<int> ids = new List<int>();

        foreach (Tratamiento tratamiento in resultadoTratamientos)
        {
            tratamientos.Add(tratamiento);
            ids.Add(tratamiento.Id);
        }

        Dictionary<int, List<TratamientoInsumo>> insumosPorTratamiento = new Dictionary<int, List<TratamientoInsumo>>();

        if (ids.Count > 0)
        {
            IEnumerable<TratamientoInsumo> resultadoInsumos = await _unitOfWork.Connection.QueryAsync<TratamientoInsumo>(
                new CommandDefinition(
                    sqlInsumos,
                    new { Ids = ids },
                    _unitOfWork.Transaction));

            foreach (TratamientoInsumo insumo in resultadoInsumos)
            {
                if (!insumosPorTratamiento.ContainsKey(insumo.TratamientoId))
                {
                    insumosPorTratamiento[insumo.TratamientoId] = new List<TratamientoInsumo>();
                }

                insumosPorTratamiento[insumo.TratamientoId].Add(insumo);
            }
        }

        foreach (Tratamiento tratamiento in tratamientos)
        {
            if (insumosPorTratamiento.ContainsKey(tratamiento.Id))
            {
                AgregarInsumos(tratamiento, insumosPorTratamiento[tratamiento.Id]);
            }
        }

        return tratamientos;
    }

    public async Task AgregarTratamiento(Tratamiento tratamiento)
    {
        if (tratamiento is null) throw new ArgumentNullException(nameof(tratamiento));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;
        if (transaccionPropia)
            await _unitOfWork.IniciarTransaccionAsync();

        try
        {
            const string sqlTratamiento = @"
INSERT INTO Tratamientos (Id, Descripcion, Costo, DuracionEstimadaTicks)
VALUES (@Id, @Descripcion, @Costo, @DuracionEstimadaTicks);";

            await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sqlTratamiento,
                    new
                    {
                        tratamiento.Id,
                        tratamiento.Descripcion,
                        Costo = tratamiento.Costo.ToString(CultureInfo.InvariantCulture),
                        DuracionEstimadaTicks = tratamiento.DuracionEstimada.Ticks
                    },
                    _unitOfWork.Transaction));

            await InsertarInsumosAsync(tratamiento, CancellationToken.None);

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

    public async Task ActualizarTratamiento(Tratamiento tratamiento)
    {
        if (tratamiento is null) throw new ArgumentNullException(nameof(tratamiento));

        bool transaccionPropia = !_unitOfWork.TransaccionActiva;
        if (transaccionPropia)
            await _unitOfWork.IniciarTransaccionAsync();

        try
        {
            const string sqlTratamiento = @"
UPDATE Tratamientos
SET Descripcion = @Descripcion,
    Costo = @Costo,
    DuracionEstimadaTicks = @DuracionEstimadaTicks
WHERE Id = @Id;";

            int filas = await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sqlTratamiento,
                    new
                    {
                        tratamiento.Id,
                        tratamiento.Descripcion,
                        Costo = tratamiento.Costo.ToString(CultureInfo.InvariantCulture),
                        DuracionEstimadaTicks = tratamiento.DuracionEstimada.Ticks
                    },
                    _unitOfWork.Transaction));

            if (filas == 0)
                throw new KeyNotFoundException($"No se encontró un tratamiento con el ID {tratamiento.Id}.");

            const string sqlEliminarInsumos = @"
DELETE FROM TratamientoInsumos
WHERE TratamientoId = @TratamientoId;";

            await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sqlEliminarInsumos,
                    new { TratamientoId = tratamiento.Id },
                    _unitOfWork.Transaction));

            await InsertarInsumosAsync(tratamiento, CancellationToken.None);

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

    public async Task<bool> EliminarTratamiento(int id)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sql = @"
DELETE FROM Tratamientos
WHERE Id = @Id;";

        int filas = await _unitOfWork.Connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, _unitOfWork.Transaction));

        return filas > 0;
    }

    public int IdAutoIncrementoSincrono()
    {
        const string sql = @"
SELECT COALESCE(MAX(Id), 0) + 1
FROM Tratamientos;";

        return _unitOfWork.Connection.ExecuteScalar<int>(
            sql,
            transaction: _unitOfWork.Transaction);
    }

    public Task<Tratamiento?> ObtenerTratamientoPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return ObtenerTratamientoPorIdInternoAsync(id, cancellationToken);
    }

    private async Task<Tratamiento?> ObtenerTratamientoPorIdInternoAsync(int id, CancellationToken cancellationToken)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException(nameof(id), "No puedes ingresar IDs negativos.");

        const string sqlTratamiento = @"
SELECT Id, Descripcion, Costo, DuracionEstimadaTicks AS DuracionEstimada
FROM Tratamientos
WHERE Id = @Id;";

        const string sqlInsumos = @"
SELECT TratamientoId, InsumoId, CantidadUsada
FROM TratamientoInsumos
WHERE TratamientoId = @Id
ORDER BY InsumoId;";

        Tratamiento? tratamiento = await _unitOfWork.Connection.QuerySingleOrDefaultAsync<Tratamiento>(
            new CommandDefinition(
                sqlTratamiento,
                new { Id = id },
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken));

        if (tratamiento is null)
            return null;

        List<TratamientoInsumo> insumos = (await _unitOfWork.Connection.QueryAsync<TratamientoInsumo>(
            new CommandDefinition(
                sqlInsumos,
                new { Id = id },
                _unitOfWork.Transaction,
                cancellationToken: cancellationToken))).ToList();

        return AgregarInsumos(tratamiento, insumos);
    }

    private async Task InsertarInsumosAsync(Tratamiento tratamiento, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO TratamientoInsumos (TratamientoId, InsumoId, CantidadUsada)
VALUES (@TratamientoId, @InsumoId, @CantidadUsada);";

        foreach (TratamientoInsumo insumo in tratamiento.InsumosRequeridos)
        {
            await _unitOfWork.Connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        TratamientoId = tratamiento.Id,
                        insumo.InsumoId,
                        insumo.CantidadUsada
                    },
                    _unitOfWork.Transaction,
                    cancellationToken: cancellationToken));
        }
    }

    private static Tratamiento AgregarInsumos(Tratamiento tratamiento, IEnumerable<TratamientoInsumo> insumos)
    {
        foreach (TratamientoInsumo insumo in insumos)
        {
            tratamiento.AgregarInsumo(
                new TratamientoInsumo(tratamiento.Id, insumo.InsumoId, insumo.CantidadUsada));
        }

        return tratamiento;
    }
}
