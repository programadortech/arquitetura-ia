# ADR-0029: Código limpo — comentários só quando necessários

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
O código gerado vinha acumulando comentários que apenas **reafirmam o que o código faz** (`// cria o usuário`,
separadores decorativos, `///` que repetem o nome do tipo). Isso polui a leitura, envelhece mal e não agrega.

## Decisão
Adotar a política de [`docs/standards/clean-code.md`](../standards/clean-code.md): **código autoexplicativo**;
comentar **apenas o "porquê" não óbvio** (decisão/trade-off, segurança, gotcha, referência a ADR/ticket).
Não comentar o "o quê". XML doc (`///`) só em membros públicos quando agrega valor e de forma concisa —
nunca repetindo o identificador.

Enforço por **prevenção** (skills/agents geram sem ruído) e por **revisão** (`tech-lead-reviewer` / `/review-pr`
tratam comentário redundante como *Should-fix*). Sem gate por script (detecção automática gera falso-positivo).

## Consequências
- (+) Código mais limpo e legível; menos manutenção de comentários desatualizados.
- (+) Padrão claro para o gerador e para a revisão.
- (−) "Necessário ou não" é julgamento — fica com o revisor, não com um script.

## Alternativas consideradas
- **Gate por script (densidade de comentários):** rejeitado — falso-positivo alto; não distingue "porquê" de "o quê".
- **Proibir todo comentário:** rejeitado — comentários de "porquê"/segurança são valiosos.

## Referências
- [`docs/standards/clean-code.md`](../standards/clean-code.md) · [`docs/standards/api-layer.md`](../standards/api-layer.md) (SRP)
