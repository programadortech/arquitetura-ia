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
   `docs/standards/usecase-dispatcher.md`, `architecture.md`, `error-handling.md`, `observability.md`, `database.md`.
   **Se a feature usa integração externa**, leia também `docs/integrations/<categoria>/README.md`.
2. **Camada Application**:
   - Tipos `XxxRequest` (input) e `XxxResponse` (output).
   - `XxxHandler : IUseCase<XxxRequest, Result<XxxResponse>>` — retorna **`Result`/`Result<T>`** com
     `Notification` para falhas de negócio. **Não use `throw`** para erro esperado (ver `error-handling.md`).
   - Defina/estenda quaisquer **portas** (interfaces) que o handler precise — nunca referencie Infrastructure.
     Integrações entram por porta do catálogo (ex.: `IEmailSender`) — provedor decidido em `docs/integrations/`.
   - Registre o handler para o dispatcher (`AddUseCase<...>()` / assembly scan).
3. **Domain**: adicione/estenda entities, value objects, domain events e invariantes conforme necessário.
4. **Infrastructure**: implemente as portas (repositório do banco, publicação em fila, adapter de integração,
   enfileiramento de job **se habilitado**), envolvendo chamadas externas em Polly e emitindo spans OTel + logs.
5. **Api**: endpoint enxuto que despacha via `IUseCaseDispatcher` e mapeia `Result` → **envelope `ApiResponse`**
   (status correto). Exceção inesperada cai no **middleware global**.
6. **Tests**: testes unitários para o handler (happy path, validações, erros como `Result.Failure`); integração
   para novos adaptadores (delegue ao `/create-tests` se for amplo).
7. Execute `dotnet build -warnaserror`, `dotnet test` e `scripts/validate-clean-architecture.ps1`.

## Standards you must enforce
- O handler implementa `IUseCase<,>`; invocado apenas através de `IUseCaseDispatcher`. Sem MediatR.
- **Erros de negócio via `Result`/`Notification` (não `throw`)**; resposta no envelope `ApiResponse`.
- **Mapeamento entidade↔model via mappers estáticos** (`ToResponse`/`ToEntity`) — **sem AutoMapper** (`docs/standards/mapping.md`).
- **Integrações pelo catálogo** (`docs/integrations/`): porta + adapter plugável.
- Async + `CancellationToken` em todo o fluxo; sem bloqueio em chamadas async.
- Logs estruturados; SQL parametrizado; inputs validados na fronteira.

## Suggested agents
`backend-developer` (líder) → `qa-tester` (testes) → `tech-lead-reviewer` + `security-reviewer`.

## Done when
Build + testes verdes, regras de arquitetura passam, e o caso de uso satisfaz seus critérios de aceitação.
