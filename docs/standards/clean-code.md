# Padrão: Código limpo — comentários, nomes e SRP

> Vinculante. Codifica o [ADR-0029](../adr/0029-codigo-limpo-comentarios.md). Aplica-se a **todo** código gerado.

## Comentários — comente o "porquê", nunca o "o quê"
O código deve ser **autoexplicativo** (nomes claros, métodos pequenos, tipos expressivos). Comentário é
exceção, não hábito.

**NÃO comente** (ruído — remover):
- O que o código já diz: `// cria o usuário` antes de `CreateUser(...)`; `// retorna` antes de `return`.
- Cabeçalhos/separadores decorativos (`// ----- serviços -----`), TODOs vagos, código comentado.
- Reafirmar nome de método/variável/intenção óbvia.

**Comente** (tem valor — manter conciso):
- O **porquê** não óbvio: motivo de uma decisão, trade-off, ordem que importa, workaround com link.
- **Segurança/risco**: por que um campo não é exposto, por que algo é genérico, mitigação aplicada.
- Referência a regra externa: `// ADR-0027: ...`, ticket, RFC, comportamento de framework não intuitivo.
- Avisos de gotcha (ex.: "PS 5.1 usa ANSI por padrão", "HMAC exige ≥ 32 bytes").

## XML doc (`///`)
- Permitido em **tipos/membros públicos** quando agrega valor (alimenta OpenAPI/IntelliSense) e for **conciso**.
- **Não** repita o nome: evite `/// <summary>Usuário do sistema</summary>` em `class Usuario`. Se o `///` só
  reescreve o identificador, **remova**.

## Nomes e tamanho
- Nomes revelam intenção; sem abreviações obscuras. Prefira renomear a comentar.
- Métodos curtos e coesos; uma responsabilidade por classe (SRP) — ver [`api-layer.md`](api-layer.md).

## Como é cobrado
Prevenção: skills/agents geram **sem** comentários supérfluos. Enforço na **revisão** (`tech-lead-reviewer` /
`/review-pr`) — comentário redundante é achado *Should-fix*. (Não há gate por script: detectar "comentário
desnecessário" automaticamente gera falso-positivo; a checagem é de julgamento.)
