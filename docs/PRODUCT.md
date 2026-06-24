# PRODUCT.md — Contexto do Produto

> Contexto vivo que a IA **lê primeiro** ao evoluir o produto. Mantenha curto e atualizado.

## Visão
- **Produto:** Plataforma2A.Auth — serviço de autenticação e identidade.
- **Domínio/segmento:** plataforma corporativa (auth/identidade).

## Estado atual
- **Solução:** criada em `src/` (monorepo, ADR-0019). Builda verde com `-warnaserror`; testes verdes.
- **Banco:** SQL Server · **Acesso a dados:** EF Core + Unit of Work (`IUnitOfWork`/`EfUnitOfWork`).
- **Fila:** RabbitMQ (abstração) · **Jobs:** none · **API docs:** Scalar + Swagger · **Gateway:** none.
- **Erros:** Result/Notification + envelope `ApiResponse` + middleware global (`GlobalExceptionHandler`).
- **Config:** por ambiente (Development/Staging/Production) + launchSettings.
- **Endpoints:** `/` (info), `/health`, `/scalar`, `/swagger`, `/openapi/v1.json` (docs fora de produção).

## Decisões-chave do produto
- Tratamento de erros: Result/Notification + envelope (ADR-0014).
- Acesso a dados: EF Core + UoW (ADR-0020).
- Sem AutoMapper — mappers estáticos (ADR-0021).
- Integrações: catálogo `docs/integrations/` (ADR-0016).

## Convenções específicas do produto
- _(a definir conforme o domínio de auth evolui)_

## Backlog / features
- Índice: [`features/README.md`](features/README.md).
- **AZ-12094 — Autenticação e Gerenciamento de Senha** — importada (a refinar). Doc: [`features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md`](features/AZ-12094-autenticacao-e-gerenciamento-de-senha.md).

## Integrações ativas
- _(nenhuma ainda — e-mail entra com o fluxo de reset de senha; decidir provedor pelo catálogo)_

---
> Atualize este arquivo ao final de cada feature/decisão relevante (faz parte do "Done").
