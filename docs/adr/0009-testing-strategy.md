# ADR-0009: Estratégia de testes (unitários + integração + arquitetura)

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Precisamos de confiança de que a lógica de negócio está correta, que as integrações de infraestrutura funcionam e que as regras
arquiteturais não podem se degradar silenciosamente.

## Decisão
Três projetos de teste por solução: **UnitTests** (handlers/domain isolados), **IntegrationTests**
(adapters contra Oracle/filas/Hangfire reais via Testcontainers) e **ArchitectureTests**
(NetArchTest garantindo a regra de dependência e as convenções). Ferramental: xUnit + FluentAssertions +
NSubstitute. Todo critério de aceitação mapeia para um teste. Regras em
[`docs/standards/testing.md`](../standards/testing.md).

## Consequências
- (+) Feedback rápido sobre a lógica, verificação real das integrações, proteção automatizada da arquitetura.
- (+) Refatorações são seguras; desvios de arquitetura quebram o build.
- (−) Testes de integração precisam de infraestrutura (containers) e rodam em um estágio de CI separado.

## Alternativas consideradas
- Apenas testes unitários: deixa passar defeitos de SQL/serialização/integração e desvios de arquitetura.
- Apenas QA manual: não é repetível, não é um gate de merge.
