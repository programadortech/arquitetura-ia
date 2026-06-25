using Microsoft.EntityFrameworkCore;

namespace Plataforma2ASmart.Auth.Infrastructure.Persistence;

/// <summary>DbContext da aplicação (SQL Server). DbSets entram com as features.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
