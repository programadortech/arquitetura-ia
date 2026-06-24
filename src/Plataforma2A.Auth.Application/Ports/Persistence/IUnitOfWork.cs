namespace Plataforma2A.Auth.Application.Ports.Persistence;

/// <summary>
/// Unit of Work — transação por caso de uso. Implementado na Infrastructure pelo ORM escolhido
/// (EF Core: <c>EfUnitOfWork</c> · Dapper: <c>DapperUnitOfWork</c>). Ver docs/standards/database.md.
/// </summary>
public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>EF Core: persiste o tracking · Dapper: commit da transação.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}
