namespace ProyectoOdontologia.Tests.Helpers;

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using global::CapaAplicacion.Interfaces;

public sealed class UnitOfWorkNulo : IUnitOfWork
{
    public IDbConnection Connection => throw new InvalidOperationException("UnitOfWorkNulo no abre conexiones.");

    public IDbTransaction? Transaction => null;

    public bool TransaccionActiva => false;

    public Task IniciarTransaccionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
