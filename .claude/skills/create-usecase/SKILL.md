---
name: create-usecase
description: Implementa um único caso de uso de ponta a ponta — request/response, handler via o dispatcher customizado, portas, adaptadores de Infrastructure, conexão de DI e testes unitários. Use quando o usuário disser "implemente o use case W".
---

# Skill: create-usecase

Implementa um caso de uso seguindo a arquitetura aprovada e os padrões do repositório.

## Inputs
- **UseCaseName** (ex: `ConfirmOrder`) e a feature/projeto proprietário.

## Steps
1. Leia `docs/architecture/<feature>.md` para o contrato deste caso de uso e os padrões em
   `docs/standards/usecase-dispatcher.md`, `architecture.md`, `observability.md`, `oracle.md`.
2. **Camada Application**:
   - Tipos `XxxRequest` (input) e `XxxResponse` (output).
   - `XxxHandler : IUseCase<XxxRequest, XxxResponse>` com validação de input e orquestração.
   - Defina/estenda quaisquer **portas** (interfaces) que o handler precise — nunca referencie Infrastructure.
   - Registre o handler para o dispatcher (`AddUseCase<...>()` / assembly scan).
3. **Domain**: adicione/estenda entities, value objects, domain events e invariantes conforme necessário.
4. **Infrastructure**: implemente as portas (repositório Oracle, publicação em fila, enfileiramento de job), envolvendo
   chamadas externas em políticas Polly e emitindo spans OTel + logs estruturados com Serilog.
5. **Api**: exponha o endpoint que despacha o request via `IUseCaseDispatcher` (controller/endpoint enxuto).
6. **Tests**: testes unitários para o handler (happy path, validação, erros); testes de integração para novos adaptadores
   (delegue ao `/create-tests` se for amplo).
7. Execute `dotnet build -warnaserror`, `dotnet test` e `scripts/validate-clean-architecture.ps1`.

## Standards you must enforce
- O handler implementa `IUseCase<,>`; invocado apenas através de `IUseCaseDispatcher`. Sem MediatR.
- Async + `CancellationToken` em todo o fluxo; sem bloqueio em chamadas async.
- Logs estruturados; SQL parametrizado; inputs validados na fronteira.

## Suggested agents
`backend-developer` (líder) → `qa-tester` (testes) → `tech-lead-reviewer` + `security-reviewer`.

## Done when
Build + testes verdes, regras de arquitetura passam, e o caso de uso satisfaz seus critérios de aceitação.
