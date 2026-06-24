# ADR-0021: Sem AutoMapper — mapeamento explícito via mappers estáticos

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Bibliotecas de mapeamento por convenção/reflexão em runtime (AutoMapper e similares) escondem o mapeamento,
quebram em runtime quando o modelo muda, custam performance (reflexão), dificultam refatoração/navegação e
viraram pagas em versões recentes. Precisamos de mapeamento explícito, seguro em tempo de compilação.

## Decisão
**Proibido AutoMapper** (e mapeadores por reflexão/convention em runtime). O mapeamento entre **entidades de
domínio** e **models/DTOs** é **explícito**, via **mappers estáticos** com **extension methods**:
- `ToResponse()` / `ToDto()` — entidade → model (saída).
- `ToEntity()` / `ToDomain()` — model/request → entidade (entrada).
- Coleções: `items.Select(x => x.ToResponse())`.

Os mappers vivem na **Application** (os DTOs são da Application), um por agregado/contexto. **Opcional:** um
**source generator** de mapeamento em **tempo de compilação** (ex.: Mapperly), que gera exatamente esse
mesmo padrão (mappers estáticos, sem reflexão em runtime) — também é aceitável; AutoMapper, não.
Regras em [`docs/standards/mapping.md`](../standards/mapping.md).

## Consequências
- (+) Mapeamento explícito, navegável e refatorável; o compilador pega campos faltando.
- (+) Sem reflexão em runtime → mais rápido e previsível; sem dependência paga.
- (+) Testável e sem "mágica".
- (−) Escrever o mapeamento à mão (mitigado por extension methods enxutas ou por um gerador compile-time).

## Alternativas consideradas
- AutoMapper: mágico, frágil em runtime, custo de reflexão, licenciamento — **rejeitado**.
- Mapeamento inline espalhado nos handlers: funciona, mas duplica e suja o caso de uso — preferimos
  centralizar em mappers estáticos por tipo.
