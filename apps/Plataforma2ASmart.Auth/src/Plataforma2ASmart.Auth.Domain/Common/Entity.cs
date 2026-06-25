namespace Plataforma2ASmart.Auth.Domain.Common;

/// <summary>Raiz de entidade com identidade tipada.</summary>
public abstract class Entity<TId>
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;
}
