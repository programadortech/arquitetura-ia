# Padrão: Estratégia de Testes

Três projetos de teste por solução, cada um em um nível distinto. Ferramentas: **xUnit + FluentAssertions +
NSubstitute** (Moq aceitável), **Testcontainers** para integração, **NetArchTest** para arquitetura.

## 1. Testes unitários — `<Project>.UnitTests`
- Alvo: handlers de casos de uso e lógica de domínio isoladamente. Portas faked/mocked.
- **Sem I/O**, sem clock/rede/filesystem reais. Rápidos e determinísticos.
- Cubra por handler: caminho feliz, cada falha de validação, casos de borda, caminhos de erro/exceção.
- Arrange/Act/Assert; um conceito lógico por teste; nomeie `Method_State_ExpectedResult`.

## 2. Testes de integração — `<Project>.IntegrationTests`
- Alvo: adapters de Infrastructure contra dependências reais.
  - Round-trips de repositório Oracle (SQL real) via **Testcontainers** ou um schema de teste descartável.
  - Round-trips de publish→consume de fila por provider configurado.
  - Enqueue/execução do Hangfire.
- Valide serialização, correção de SQL, transações e idempotência.
- Limpe o estado entre testes; sem fixtures mutáveis compartilhadas que causem dependência de ordem.

## 3. Testes de arquitetura — `<Project>.ArchitectureTests`
- Imponha as regras de `architecture.md` com NetArchTest, ex.:
  - Domain não depende de nada externo.
  - Application **não** referencia Infrastructure.
  - Handlers nomeados `*Handler` e que implementam `IUseCase<,>`.
  - Nenhum tipo referencia `MediatR` nem `AutoMapper`.
  - Tipos do SDK do provider vivem apenas sob `Infrastructure.Messaging`.

## Cobertura e barra de qualidade
- Todo critério de aceitação (Given/When/Then) mapeia para ≥1 teste.
- Um caso de uso sem teste unitário está **incompleto** e bloqueia o PR.
- Os testes devem ser determinísticos — sem corridas de `Thread.Sleep`, sem `DateTime.Now` real (injete um clock).
- Rode em CI: unit + arquitetura em todo PR; integração em um estágio dedicado.

## Comandos
```
dotnet test                                   # all
dotnet test tests/<Project>.UnitTests         # fast loop
pwsh scripts/validate-tests.ps1               # gate
```
Veja [ADR-0009](../adr/0009-testing-strategy.md). Escreva testes via `/create-tests`.
