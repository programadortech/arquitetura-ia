# PRODUCT.md — Contexto do Produto

> Contexto vivo que a IA **lê primeiro** ao evoluir o produto. Mantenha curto e atualizado: reflita o
> estado real, as decisões-chave e as convenções. Detalhes ficam em features/architecture/ADRs.

## Visão
- **Produto:** _(a definir — nome e propósito em uma linha)_
- **Domínio/segmento:** _(a definir)_

## Estado atual
- **Solução:** _(ainda não criada — rode `/create-project` para gerar em `src/`)_
- **Banco:** _(provider escolhido)_ · **Fila:** _(provider)_ · **Jobs:** _(hangfire|none)_ ·
  **API docs:** _(scalar/swagger)_ · **Gateway:** _(yarp|none)_

## Decisões-chave do produto
- Tratamento de erros: **Result/Notification + envelope** (ADR-0014).
- Integrações: plugáveis, decididas pelo catálogo `docs/integrations/` (ADR-0016).
- _(adicione decisões específicas do produto aqui)_

## Convenções específicas do produto
- _(nomenclatura, contextos/bounded contexts, regras transversais próprias)_

## Backlog / features
- Índice: [`features/README.md`](features/README.md).

## Integrações ativas
- _(ex.: E-mail → SMTP (dev); Pagamentos → Pagar.me — preencher conforme adotadas)_

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
