namespace Plataforma2A.Auth.Application.Abstractions;

/// <summary>Retorno para casos de uso sem valor (comando).</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
