using Microsoft.EntityFrameworkCore;

namespace Plataforma2A.Auth.Infrastructure.Persistence;

/// <summary>DbContext da aplicação (SQL Server). DbSets entram com as features. Banco plugável (ADR-0013).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
