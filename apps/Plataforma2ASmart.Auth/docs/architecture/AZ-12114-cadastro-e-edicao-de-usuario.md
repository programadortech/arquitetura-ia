# Arquitetura: Cadastro e Edição de Usuário

- **Feature:** [`../features/AZ-12114-cadastro-e-edicao-de-usuario.md`](../features/AZ-12114-cadastro-e-edicao-de-usuario.md)
- **Status:** Aprovada · **Data:** 2026-06-25 · **Branch:** `feature/import-az-12114`
- **ADRs:** [P0002 — Administração de usuários + autorização](../adr/0002-administracao-usuarios-autorizacao.md) (produto); herda P0001 (Identity+JWT); transversais 0014/0028/0020/0029/0030.

## 1. Resumo
Administração de usuários sobre o **ASP.NET Core Identity** já existente (AZ-12094): dois casos de uso
(`CreateUser`, `UpdateUser`) na Application retornando **`Result<T>`** (BuildingBlocks). Uma porta dedicada
`IUserAdminService` encapsula `UserManager`/`RoleManager` (unicidade, política de senha, geração de senha
temporária, associação de roles **transacional**); `IUserWelcomeEmailSender` prepara o e-mail de boas-vindas
(impl. temporária com `TODO`). A Api expõe `POST/PUT /api/users` por um `UsersController` fino protegido pela
**policy `Users.Manage`**. Inativos são barrados no login pelo gate `IsActive` no fluxo da AZ-12094.

## 2. Casos de uso (todos retornam Result)
| Use case | Request → Response | Efeitos | AC |
|---|---|---|---|
| `CreateUser` | `CreateUserRequest(Name, Email, UserName, Password?, Roles[], IsActive)` → `Result<UserResponse>` | cria usuário no Identity (senha informada **ou** temporária gerada), valida unicidade/política/roles, associa roles (**transacional**), prepara e-mail de boas-vindas se sem senha | #1–#5, #10, #11, #12 |
| `UpdateUser` | `UpdateUserRequest(UserId, Name, Email, UserName, Roles[], IsActive)` → `Result<UserResponse>` | atualiza perfil + roles + `IsActive` (**não** altera senha); revalida unicidade contra outros usuários | #6, #7, #8, #10, #11, #12 |

`UserResponse(Id, Name, Email, UserName, Roles[], IsActive, TemporaryPasswordGenerated)` — `TemporaryPasswordGenerated` só é `true` no create sem senha; a senha temporária **nunca** vai no response nem em log.

## 3. Camadas (consumindo BuildingBlocks)
- **Domain:** sem nova entidade — o "usuário" é um agregado do Identity (Infrastructure), como na AZ-12094. As regras de negócio vivem nos handlers + porta.
- **Application:**
  - Handlers `CreateUserHandler`, `UpdateUserHandler` (`IUseCase<,>` → `Result`).
  - Portas novas: **`IUserAdminService`** (`Ports/Users/`) e **`IUserWelcomeEmailSender`** (`Ports/Email/`).
  - `IUserAdminService` retorna **outcomes** que o handler mapeia para `Error` (sem vazar `IdentityResult`):
    `CreateAsync(CreateUserSpec) → UserCreateOutcome(Succeeded, UserId?, TemporaryPassword?, Error[])`,
    `UpdateAsync(UpdateUserSpec) → UserUpdateOutcome(Succeeded, Error[])`. Erros possíveis: `DuplicateEmail`,
    `DuplicateUserName`, `PasswordPolicy`, `RoleNotFound`, `UserNotFound`.
  - Commit via `IUnitOfWork` (BuildingBlocks); e-mail de boas-vindas chamado **após** o commit, falha não derruba o cadastro.
- **Infrastructure:**
  - `ApplicationUser` ganha **`Name`** (string) e **`IsActive`** (bool, default `true`).
  - `UserAdminService : IUserAdminService` sobre `UserManager<ApplicationUser>` + `RoleManager<IdentityRole<Guid>>`;
    cria usuário + associa roles dentro de **uma transação** (rollback se a associação falhar); valida existência de roles;
    gera **senha temporária** compatível com `IdentityOptions.Password` quando não informada.
  - `UserWelcomeEmailSender : IUserWelcomeEmailSender` — impl. temporária: loga "boas-vindas preparado" (sem senha) + `TODO`.
  - **Gate de inativo:** `IIdentityService.ValidateCredentialsAsync` (AZ-12094) passa a recusar `!IsActive`.
- **Api:** `UsersController` fino; `POST` sob `[Authorize(Policy="Users.Create")]` (bootstrap), `PUT` sob `[Authorize(Policy="Users.Manage")]`;
  contratos em `Contracts/Users/` (`ToUseCase()`); policies + `CreateUserAuthorizationHandler` em `Authorization/`. Roles semeadas no startup (`SeedRolesAsync`).

### Bootstrap do primeiro usuário
`Users.Create` (requirement `CreateUserRequirement` + handler) libera o `POST` para **admin OU sistema sem nenhum usuário**
(`IUserAdminService.AnyUserExistsAsync`). Fecha sozinho ao criar o primeiro. `PUT` nunca é liberado por bootstrap.

```
Api(UsersController) ─▶ Application(Create/UpdateUser, IUserAdminService, IUserWelcomeEmailSender) ─▶ Domain(—)
        └────────────▶ Infrastructure(UserAdminService, ApplicationUser, UserWelcomeEmailSender) ─┘  (implementa as ports)
```

## 4. Modelo de dados (SQL Server)
- **`AspNetUsers`** ganha colunas **`Name`** (`nvarchar(256)`, nullable inicialmente p/ usuários existentes) e
  **`IsActive`** (`bit NOT NULL DEFAULT 1`).
- **Unicidade** de e-mail e login já garantida pelos índices únicos do Identity (`NormalizedEmail`, `NormalizedUserName`) — consultas por e-mail/login usam esses índices (NFR de performance atendido).
- **Roles** em `AspNetRoles`; seed de `Administrador`/`Operador`/`Supervisor` (a confirmar) — a feature não cria roles em runtime.
- **Plano de migração:** `db/sqlserver/migrations/0002_az-12114_user_profile_fields.sql` (+ `.down.sql`), criado via `/create-db-script` (adiciona `Name`+`IsActive`; down remove as colunas).

## 5. Erros (Result → envelope)
| Situação | Error.Type → HTTP |
|---|---|
| E-mail duplicado / login duplicado | `Conflict` → 409 |
| Senha fora da política / entrada inválida / role inexistente | `Validation` → 400 |
| Usuário não encontrado (edição) | `NotFound` → 404 |
| Sem autenticação / sem policy `Users.Manage` | `401` / `403` (middleware de authz) |
| Criar (sucesso) | `201 Created` (+ `Location: /api/users/{id}`) |
| Editar (sucesso) | `200 OK` |

## 6. Segurança / Resiliência / Observabilidade
- **AuthZ** por policy `Users.Manage`; senha sempre com hash do Identity; **senha temporária só em memória**, nunca em log/response.
- Acesso a banco sob o pipeline **Polly `database`** (já registrado na Infra). Criação usuário+roles **transacional** (`IUnitOfWork`/transação).
- **Logs estruturados** (templates + propriedades, sem dados sensíveis):
  - `Tentativa de cadastro de usuário {UserName}` · `Usuário {UserId} cadastrado` · `Falha no cadastro de usuário {UserName} {Erros}`
  - `Tentativa de edição {UserId}` · `Usuário {UserId} editado` · `Falha na edição {UserId} {Erros}`
  - `Roles do usuário {UserId} atualizadas` · `E-mail de boas-vindas preparado para {UserName}` (sem senha).
- **Métricas** (OpenTelemetry, contadores): `users.created`, `users.create_failed`, `users.updated`, `users.update_failed`, `users.temp_password_generated`, `users.welcome_email_prepared`.
- **Spans:** `CreateUser`, `UpdateUser` (atributos `user.id`, `roles.count`, `temp_password.generated`).

## 7. Plano de tasks
ApplicationUser `Name`+`IsActive` · porta `IUserAdminService` + outcomes · porta `IUserWelcomeEmailSender` ·
`CreateUserHandler` · `UpdateUserHandler` · `UserAdminService` (UserManager/RoleManager, transação, senha temp) ·
`UserWelcomeEmailSender` (temporário) · gate `IsActive` no login (regressão AZ-12094) · `UsersController` + contratos +
policy `Users.Manage` · migração SQL Server `0002` · testes (unit handlers + arquitetura + regressão login) · observabilidade.

---
> Próximo: `/create-usecase CreateUser` e `/create-usecase UpdateUser`; migração com `/create-db-script`; testes com `/create-tests`. PR para `dev`.
