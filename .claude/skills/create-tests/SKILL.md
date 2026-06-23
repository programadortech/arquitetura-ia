---
name: create-tests
description: Escreve a suíte de testes de uma feature/caso de uso — testes unitários, de integração e de arquitetura — mapeados aos critérios de aceitação. Use quando o usuário pedir testes ou cobertura de código existente.
---

# Skill: create-tests

Produz testes determinísticos no nível correto e comprova que os critérios de aceitação estão cobertos.

## Inputs
- A feature/caso de uso a cobrir (e seus critérios de aceitação de `docs/features/<feature>.md`).

## Steps
1. Adote a mentalidade do `qa-tester`; leia `docs/standards/testing.md`. Preencha `templates/test-plan-template.md`.
2. **Testes unitários** (`<Project>.UnitTests`): cada handler de caso de uso — happy path, falhas de validação,
   casos de borda, caminhos de erro. Portas fakeadas/mockadas (NSubstitute/Moq). Sem I/O, sem clock/rede reais.
3. **Testes de integração** (`<Project>.IntegrationTests`): novos adaptadores — round-trip do repositório Oracle
   (Testcontainers ou schema de teste), publish/consume de fila, enqueue no Hangfire. Serialização & SQL reais.
4. **Testes de arquitetura** (`<Project>.ArchitectureTests`): regras NetArchTest — regra de dependência,
   "Application não tem referência a Infrastructure", nomenclatura de handlers, sem MediatR.
5. Use xUnit + FluentAssertions; Arrange/Act/Assert; nomes descritivos `Method_State_Expected`.
6. Execute `dotnet test` e `scripts/validate-tests.ps1`.

## Suggested agents
`qa-tester` (líder) → `tech-lead-reviewer`.

## Done when
Todo critério de aceitação mapeia para ≥1 teste, todos os testes passam de forma determinística, e `validate-tests.ps1` está verde.
