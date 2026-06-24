# ADR-0026: Gestão administrativa de usuários (Identity) — IsActive, criação transacional e porta de e-mail

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude
- **Feature:** [AZ-12114 — Cadastro e Edição de Usuário](../features/AZ-12114-cadastro-e-edicao-de-usuario.md)

## Contexto
A AZ-12114 adiciona administração de usuários sobre o ASP.NET Core Identity já adotado ([ADR-0024](0024-identity-jwt-autenticacao.md)):
cadastrar/editar usuário, perfis (roles), status ativo/inativo e senha opcional (gerando temporária quando ausente).
O work item também pede uma **interface** de e-mail de boas-vindas, com o envio real adiado para outra história.

Três pontos precisam de decisão arquitetural: (1) como representar "ativo/inativo" e impedir login de inativo;
(2) como garantir que criar usuário + associar roles não deixe usuário "pela metade"; (3) como isolar o envio de e-mail
que ainda não será implementado.

## Decisão
1. **`ApplicationUser` ganha `Name` (nome completo) e `IsActive` (bool).** Uma migração adiciona as colunas em
   `AspNetUsers`. A validação de credenciais (porta `IIdentityService`, AZ-12094) passa a **recusar usuário inativo**.
2. **Criação é transacional via `IUnitOfWork`** ([ADR-0020](0020-data-access-efcore-or-dapper-uow.md)): `UserManager.CreateAsync`
   + associação de roles num único escopo transacional; falha em qualquer etapa ⇒ rollback (sem usuário parcial).
   Quando a senha não é informada, uma **senha temporária** é gerada seguindo a política do Identity (só em memória).
3. **Porta dedicada `IUserWelcomeEmailSender`** (contrato do work item) na Application; o adapter na Infrastructure é um
   **stub** que apenas loga "envio preparado" (sem a senha) e marca `// TODO` para o envio real. Falha de e-mail **não**
   aborta o cadastro — é apenas registrada.
4. **Operações de admin atrás da porta `IUserAdminService`** (sobre `UserManager`/`RoleManager`), mantendo a Application
   livre de tipos do Identity. Os endpoints `POST /api/users` e `PUT /api/users/{id}` exigem a **policy `users:manage`**
   (satisfeita pela role `Administrador`). Casos de uso retornam **`Result<T>`**; a Api responde no envelope `ApiResponse`.

## Consequências
- (+) Status ativo/inativo explícito e aplicado no login; auditável.
- (+) Sem usuário criado parcialmente (atomicidade criação + roles).
- (+) Envio de e-mail isolado por porta — a história futura só implementa o adapter, sem tocar no caso de uso.
- (+) Application continua sem depender do Identity (ports isolam `UserManager`/`RoleManager`).
- (−) Migração altera `AspNetUsers` (colunas `Name`, `IsActive`) — exige script reversível e deploy coordenado.
- (−) `IsActive` duplica parcialmente o conceito de lockout do Identity; optou-se por flag explícita pela clareza de negócio.

## Alternativas consideradas
- **Usar `LockoutEnd`/`LockoutEnabled` do Identity para "inativo":** reaproveita o Identity, mas mistura "bloqueio temporário
  por tentativas" com "status administrativo" — menos claro para o negócio. Rejeitada em favor de `IsActive` explícito.
- **Reusar `IEmailSender` (AZ-12094) em vez de `IUserWelcomeEmailSender`:** o work item define o contrato dedicado;
  mantém o boas-vindas desacoplado do e-mail transacional. A implementação futura pode delegar ao `IEmailSender`.
- **Sem transação (UserManager em chamadas soltas):** simples, mas permite usuário sem roles em caso de falha. Rejeitada.

## Referências
- [AZ-12114 (feature)](../features/AZ-12114-cadastro-e-edicao-de-usuario.md) · [arquitetura](../architecture/AZ-12114-cadastro-e-edicao-de-usuario.md)
- [ADR-0024 — Identity + JWT](0024-identity-jwt-autenticacao.md) · [ADR-0020 — UoW](0020-data-access-efcore-or-dapper-uow.md) · [ADR-0014 — Result/envelope](0014-error-handling-result-notification.md)
