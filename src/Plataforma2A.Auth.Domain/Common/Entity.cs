namespace Plataforma2A.Auth.Domain.Common;

/// <summary>Base de entidade de domínio com identidade. Sem dependências externas.</summary>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>Identidade da entidade.</summary>
    public TId Id { get; protected set; } = default!;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();
}
