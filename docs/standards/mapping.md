# Padrão: Mapeamento (sem AutoMapper)

Mapeamento entre **entidades de domínio** e **models/DTOs** é **explícito**, via **mappers estáticos** com
**extension methods**. **AutoMapper é proibido** (e qualquer mapeador por reflexão/convention em runtime).
Ver [ADR-0021](../adr/0021-no-automapper-static-mappers.md).

## Forma
- Um mapper estático por agregado/contexto, na **Application** (os DTOs são da Application).
- **Convenção de métodos:**
  - `ToResponse()` / `ToDto()` — **entidade → model** (saída).
  - `ToEntity()` / `ToDomain()` — **model/request → entidade** (entrada).
  - Coleções: `items.Select(x => x.ToResponse()).ToList()`.

```csharp
namespace Plataforma2A.Application.UseCases.Pedidos.Mapping;

public static class PedidoMappings
{
    // entidade -> model (saída)
    public static PedidoResponse ToResponse(this Pedido pedido) => new(
        pedido.Id,
        pedido.ClienteId,
        pedido.Total,
        pedido.Status.ToString());

    // request/model -> entidade (entrada)
    public static Pedido ToEntity(this CriarPedidoRequest request) =>
        Pedido.Criar(request.ClienteId, request.Itens.Select(i => i.ToEntity()).ToList());
}
```

Uso no handler:
```csharp
var pedido = request.ToEntity();
await _pedidos.AddAsync(pedido, ct);
await _unitOfWork.SaveChangesAsync(ct);
return Result<PedidoResponse>.Success(pedido.ToResponse());
```

## Regras
- **Nunca** referenciar `AutoMapper` (imposto pelo hook `post-edit-check` e por teste de arquitetura no projeto).
- Mapeamento é **explícito** — nada de convenção mágica; o compilador deve pegar campos faltando.
- Não colocar regra de negócio no mapper (só transformação de forma).
- Entidade nunca expõe setters públicos só para o mapper — construir via fábrica/ctor do domínio.

## Opcional (compile-time)
Um **source generator** de mapeamento (ex.: **Mapperly**) é aceitável: gera o mesmo padrão de mappers
estáticos `ToX()` em **tempo de compilação**, sem reflexão em runtime. AutoMapper continua **proibido**.
