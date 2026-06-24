# Arquitetura: Cadastro e Edição de Usuário

- **Feature:** [`../features/AZ-12114-cadastro-e-edicao-de-usuario.md`](../features/AZ-12114-cadastro-e-edicao-de-usuario.md)
- **Status:** Aprovada · **Data:** 2026-06-24 · **Branch:** `feature/12114-cadastro-e-edicao-de-usuario`
- **ADRs:** [0026 — Gestão administrativa de usuários](../adr/0026-gestao-administrativa-de-usuarios.md) (+ 0024 Identity, 0014 erros, 0020 UoW, 0021 mappers)

## 1. Resumo
Administração de usuários sobre **ASP.NET Core Identity**: cadastrar (senha opcional → gera temporária),
editar dados básicos + roles, ativar/inativar. Casos de uso retornam **`Result<T>`** (sem throw); a Api responde
no **envelope `ApiResponse`**. Criação + roles é **transacional** (`IUnitOfWork`). Ports isolam o Identity
(`IUserAdminService`) e o e-mail de boas-vindas (`IUserWelcomeEmailSender`, stub com `TODO`). Endpoints exigem
a **policy `users:manage`** (role `Administrador`).

## 2. Casos de uso (todos retornam Result)
| Use case | Request → Response | Efeitos | AC |
|---|---|---|---|
| `CreateUser` | `CreateUserRequest(name,email,userName,password?,roles[],isActive)` → `Result<UserResponse>` | cria no Identity (com senha informada ou **temporária** gerada), associa roles, prepara e-mail de boas-vindas se gerou senha; transacional | #1–#5 |
| `UpdateUser` | `UpdateUserRequest(id,name,email,userName,roles[],isActive)` → `Result<UserResponse>` | atualiza dados básicos + sincroniza roles + status; **não** altera senha | #6, #7, #8 |

`UserResponse(Id, Name, Email, UserName, Roles[], IsActive, TemporaryPasswordGenerated)` — mapeada por **mapper estático** (`ToResponse`, ADR-0021).

## 3. Camadas
- **Domain:** sem entidade nova (usuário é do Identity). Eventuais invariantes de negócio ficam nos handlers.
- **Application:**
  - Handlers `CreateUserHandler`, `UpdateUserHandler` (`IUseCase<,>` → `Result`).
  - Ports: **`IUserAdminService`** (criar com/sem senha, gerar senha temporária, verificar e-mail/login duplicado,
    atualizar dados, sincronizar roles, ativar/inativar, buscar por id) e **`IUserWelcomeEmailSender`**.
  - Usa `IUnitOfWork` (já existente) para a transação de criação.
  - `UserResponse` + mappers estáticos.
- **Infrastructure:**
  - `ApplicationUser` estendido: **`Name`** (string) e **`IsActive`** (bool, default true).
  - `UserAdminService : IUserAdminService` sobre `UserManager<ApplicationUser>` + `RoleManager<IdentityRole<Guid>>`;
    senha temporária via gerador que respeita a política do Identity.
  - `IIdentityService.ValidateCredentialsAsync` (AZ-12094) passa a **negar usuário inativo**.
  - `UserWelcomeEmailSender : IUserWelcomeEmailSender` — **stub**: loga "envio preparado" (sem a senha) + `// TODO`.
- **Api:**
  - Endpoints `POST /api/users` e `PUT /api/users/{id}` → despacham via `IUseCaseDispatcher`, mapeiam `Result → ApiResponse`.
  - `.RequireAuthorization("users:manage")`; policy registrada exigindo role `Administrador`.
  - Validação de entrada (campos obrigatórios, formato de e-mail).

## 4. Modelo de dados (SQL Server)
- Reusa as tabelas do **Identity**. **Migração** adiciona em `AspNetUsers`:
  - `Name NVARCHAR(256) NULL` (ou NOT NULL com default), `IsActive BIT NOT NULL DEFAULT 1`.
  - Índices já existentes de e-mail/usuário normalizados cobrem as consultas de unicidade.
- Script reversível em `db/sqlserver/migrations` (`..._az-12114_user_name_isactive.sql` + `.down.sql`).

## 5. Erros (Result → envelope)
| Situação | Error.Type → HTTP |
|---|---|
| E-mail ou usuário/login duplicado | `Conflict` → 409 |
| Senha fora da política / validação de entrada | `Validation` → 400 |
| Usuário inexistente (edição) | `NotFound` → 404 |
| Sem permissão (`users:manage`) | `Forbidden` → 403 / não autenticado → 401 |
| Sucesso | 200 (criação pode retornar 201) |

## 6. Confiabilidade / Segurança / Observabilidade
- **Transacional:** `CreateUser` envolve criação + roles em `IUnitOfWork`; falha ⇒ rollback (sem usuário parcial).
- **Senha temporária** só em memória; **nunca** logada. Senhas via hash do Identity.
- **E-mail indisponível não falha o cadastro** — falha apenas registrada (log de acompanhamento).
- **Inativo não autentica** (checado na validação de credenciais).
- **Logs estruturados** (Serilog) para tentativa/sucesso/falha de cadastro e edição, associação de roles e preparação de e-mail
  (sem PII/senha); **métricas** (OpenTelemetry): cadastrados, falhas de cadastro, editados, falhas de edição, senhas temporárias, e-mails preparados.
- Acesso a banco sob pipeline Polly `database`.

## 7. Autorização
- **`POST /api/users` é PÚBLICO/anônimo** ([ADR-0027](../adr/0027-cadastro-de-usuario-publico.md)): o corpo **não** aceita
  `roles` nem `isActive`; o servidor fixa a role padrão **`Usuario`** e `isActive=true` (evita escalada de privilégio).
  **Sem rate limit** nesta entrega (decisão do PO; mitigação recomendada como follow-up).
- **`PUT /api/users/{id}` exige a policy `users:manage`** (role `Administrador`) — atribuição/elevação de roles só aqui.

## 8. Plano de tasks (write-back via /sync-tasks)
Estender `ApplicationUser` (Name/IsActive) · migração SQL Server · ports `IUserAdminService`/`IUserWelcomeEmailSender` ·
`UserAdminService` + gerador de senha temporária · `UserWelcomeEmailSender` (stub) · ajuste do login p/ negar inativo ·
`CreateUser`/`UpdateUser` handlers (Result) + `UserResponse`/mappers · endpoints + policy `users:manage` · testes (unit + integração) · observabilidade.

---
> Próximo: `/create-usecase` por handler (CreateUser, UpdateUser); `/create-db-script` para a migração; `/create-tests`. PR para `dev`.
