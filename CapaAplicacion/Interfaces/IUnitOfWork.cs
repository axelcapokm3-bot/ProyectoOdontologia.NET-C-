namespace CapaAplicacion.Interfaces;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }

    IDbTransaction? Transaction { get; }

    bool TransaccionActiva { get; }

    Task IniciarTransaccionAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        IniciarTransaccionAsync(cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);
}
