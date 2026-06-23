# Checklist de Qualidade (todo PR)

Copie os itens relevantes para a descrição do PR (`templates/pr-template.md`). O hook `pre-pr-check` +
`scripts/validate-pr.ps1` impõem as partes automatizáveis.

## Arquitetura
- [ ] Regra de dependência respeitada (Domain ← Application ← {Infrastructure, Api}).
- [ ] Application não referencia Infrastructure.
- [ ] Casos de uso despachados via `IUseCaseDispatcher`; handlers implementam `IUseCase<,>`.
- [ ] Nenhum `MediatR` (ou outra dependência paga) introduzido.
- [ ] Novas decisões cross-cutting registradas como um ADR.

## Qualidade de código
- [ ] Compila com `-warnaserror`; `dotnet format` limpo.
- [ ] Async de ponta a ponta; `CancellationToken` propagado; sem `.Result`/`.Wait()`.
- [ ] Entradas validadas no limite do caso de uso/endpoint.
- [ ] Sem código morto, sem blocos comentados, sem TODOs sem uma issue rastreada.

## Dados (Oracle)
- [ ] Todo SQL parametrizado (bind variables).
- [ ] Scripts de migração versionados + nomeados conforme o padrão; script de rollback presente ou justificado.
- [ ] Sem DDL destrutivo desprotegido.
- [ ] Índices/constraints justificados.

## Resiliência
- [ ] Toda chamada externa tem um timeout.
- [ ] Retries limitados, somente transientes, com backoff + jitter; escritas/publicações idempotentes.
- [ ] Mensagens venenosas enviadas para dead-letter após tentativas limitadas.

## Observabilidade
- [ ] Logs estruturados (message templates + propriedades); sem strings de log interpoladas.
- [ ] Spans no ingresso da API, casos de uso, DB, fila, jobs; trace context propagado através da mensageria.
- [ ] Métricas-chave emitidas; sem labels de alta cardinalidade.
- [ ] Sem segredos/PII em logs ou telemetria.

## Testes
- [ ] Testes unitários para todo caso de uso novo/alterado (feliz + falha + borda).
- [ ] Testes de integração para novos adapters.
- [ ] Testes de arquitetura ainda passam.
- [ ] Cada critério de aceitação mapeia para um teste.

## Segurança
- [ ] Sem segredos no código-fonte/logs; configuração via env/secret store.
- [ ] AuthN/AuthZ imposta; menor privilégio.
- [ ] Entradas validadas; mensagens consumidas validadas; sem vetores de injeção.

## Docs
- [ ] Docs de feature/arquitetura atualizados.
- [ ] ADR adicionado se uma decisão foi tomada.
- [ ] Descrição do PR completa (`templates/pr-template.md`).
