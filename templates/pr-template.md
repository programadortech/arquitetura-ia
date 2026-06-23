# Pull Request

> Também usado como `.github/pull_request_template.md` nos projetos gerados.

## O quê e por quê
<Descrição curta da mudança e da motivação. Linke o documento de feature/arquitetura e os ADRs.>

- Feature: <docs/features/…>
- Arquitetura: <docs/architecture/…>
- ADRs: <…>
- Closes: <#issue>

## Tipo de mudança
- [ ] Novo caso de uso / feature
- [ ] Correção de bug
- [ ] Migração Oracle
- [ ] Infraestrutura / DevOps
- [ ] Apenas Docs / ADR

## Como testar
<Passos / comandos que um revisor executa para verificar.>

## Checklist de qualidade
Veja [`docs/standards/quality-checklist.md`](../docs/standards/quality-checklist.md). Confirme:

**Arquitetura**
- [ ] Regra de dependência respeitada; Application não referencia Infrastructure.
- [ ] Casos de uso via `IUseCaseDispatcher`; sem MediatR / dependências pagas.
- [ ] ADR adicionado para qualquer nova decisão.

**Código e dados**
- [ ] Compila com `-warnaserror`; `dotnet format` limpo.
- [ ] Async + `CancellationToken`; entradas validadas.
- [ ] SQL Oracle parametrizado; migrações versionadas + reversíveis; sem DDL destrutivo desprotegido.

**Resiliência e observabilidade**
- [ ] Timeouts + retries limitados em chamadas externas; escritas/publicações idempotentes; DLQ para mensagens venenosas.
- [ ] Logs estruturados; spans + métricas; contexto de trace propagado; sem segredos/PII na telemetria.

**Testes**
- [ ] Testes unitários para os casos de uso alterados; testes de integração para novos adapters; testes de arquitetura verdes.
- [ ] Cada critério de aceite mapeia para um teste.

**Segurança**
- [ ] Sem segredos no código-fonte/logs; authz aplicada; sem vetores de injection.

## Gate automatizado
- [ ] `bash .claude/hooks/pre-pr-check.sh` passa (executa todos os `scripts/validate-*.ps1`).

## Screenshots / notas
<opcional>
