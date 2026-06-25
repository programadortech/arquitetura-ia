# Feature: Cadastro e Edição de Usuário

- **Status:** Arquitetura aprovada
- **Responsável:** —
- **Data:** 2026-06-25
- **Relacionados:**
  - Work item: [AZ-12114](https://dev.azure.com/T-SystemsdoBrasil/Yamaha%20-%20Rollout/_workitems/edit/12114) (Product Backlog Item · Sprint 16)
  - Produto: [Plataforma2ASmart.Auth](../PRODUCT.md)
  - Predecessora: [AZ-12094 — Autenticação e Gerenciamento de Senha](AZ-12094-autenticacao-e-gerenciamento-de-senha.md) (Identity + JWT já existentes)

## Descrição do problema
Como administrador do sistema, quero **cadastrar e editar usuários** utilizando ASP.NET Core Identity,
para permitir o controle de acesso ao sistema de forma segura e padronizada.

Hoje o produto autentica e gerencia senha (AZ-12094), mas não há um fluxo administrativo para criar
usuários, definir seus perfis (roles) e ativar/inativar acesso. Esta feature entrega esse fluxo de
administração de usuários.

## Contexto de negócio
O acesso ao sistema é controlado por **roles/perfis** (ex.: Administrador, Operador, Supervisor). A gestão
desses usuários precisa ser feita por um administrador, com regras de unicidade (e-mail e login), política
de senha do Identity e a possibilidade de provisionar usuários **sem senha** — caso em que o sistema gera
uma senha temporária e prepara o envio de um e-mail de boas-vindas.

O **envio real do e-mail não faz parte desta história**: ela apenas define a interface e uma implementação
temporária (com `TODO`) para uma história futura concluir.

## Resultados / métricas de sucesso
- Administrador consegue cadastrar um usuário (com ou sem senha) e ele passa a poder autenticar (se ativo).
- 100% dos cadastros sem senha geram senha temporária conforme a política do Identity e preparam o e-mail de boas-vindas.
- Zero usuários duplicados por e-mail ou login.
- Cadastro responde em ≤ 500 ms e edição em ≤ 300 ms (condições normais, desconsiderando envio de e-mail).
- Métricas expostas: usuários cadastrados/editados, falhas de cadastro/edição, senhas temporárias geradas, e-mails de boas-vindas preparados.

## Escopo
**No escopo**
- Cadastro de usuário (`POST /api/users`) com: Nome, E-mail, Usuário/Login, Roles/perfis, Status ativo/inativo, Senha **opcional**.
- Quando a senha **não** é informada: geração de senha temporária (política do Identity) + preparação do e-mail de boas-vindas.
- Edição de dados básicos (`PUT /api/users/{id}`): Nome, E-mail, Usuário/Login, Roles/perfis, Status ativo/inativo.
- Atualização dos vínculos de roles no Identity ao editar.
- Ativação/inativação de usuário; usuário inativo **não autentica**.
- Unicidade de e-mail e de nome de usuário.
- **Interface** de envio de e-mail de boas-vindas + implementação temporária com `TODO` (sem envio real).
- Validação de entrada, autenticação + autorização via **policy nomeada `Users.Manage`** em todos os endpoints.
- Logs estruturados (sem dados sensíveis) e métricas.

**Fora do escopo**
- Envio real do e-mail de boas-vindas (história futura — apenas preparar/registrar).
- Alteração de senha pela edição de usuário (senha é tratada apenas pelos fluxos de senha da AZ-12094).
- Exclusão de usuário, fluxo de listagem/paginação e tela/UI (frontend).
- Gestão de roles em si (criação de roles novas) além de associar as existentes ao usuário.

## Regras de negócio (decididas no refinamento)
- **Autorização:** os endpoints exigem a **policy nomeada `Users.Manage`**, mapeada para a(s) role(s) administrativa(s) — o controller não depende do nome literal de uma role.
- **Roles informadas devem existir:** informar uma role inexistente no cadastro/edição **recusa** a operação com erro de **Validation** (a feature **não** cria roles novas).
- **Status ativo/inativo:** modelado como flag **`IsActive`** no `ApplicationUser`; o **gate de login** (fluxo da AZ-12094) recusa usuários inativos. Inativar **não** apaga o usuário.
- **Nome:** atributo de perfil **`Name`** adicionado ao `ApplicationUser` (o Identity padrão não possui `Name`).
- **Senha opcional:** com senha → cria com a senha informada (validada pela política do Identity); sem senha → gera senha temporária (política do Identity), mantida **só em memória**, e prepara o e-mail de boas-vindas.
- **E-mail de boas-vindas:** via **porta dedicada `IUserWelcomeEmailSender`** (Application); implementação temporária na Infrastructure apenas **loga** (sem expor a senha) com `TODO` para a história futura. Indisponibilidade do e-mail **não** falha o cadastro.
- **Unicidade:** e-mail e login únicos (normalizados pelo Identity) — duplicidade recusa com mensagem específica.
- **Transacional:** criação do usuário + associação de roles é atômica (Unit of Work); falha na associação **não** deixa usuário criado parcialmente.
- **Edição não altera senha:** senha permanece exclusiva dos fluxos de senha da AZ-12094.

## Critérios de aceite (Given/When/Then)
1. **Given** um administrador autenticado **when** cadastra um usuário com dados obrigatórios **e** uma senha válida **then** o sistema cria o usuário no Identity, associa as roles, retorna os dados do usuário e **não** gera senha temporária.
2. **Given** um administrador autenticado **when** cadastra um usuário com dados obrigatórios **sem** informar senha **then** o sistema gera uma senha temporária válida (política do Identity), cria o usuário, associa as roles, **prepara** o e-mail de boas-vindas e retorna os dados (`temporaryPasswordGenerated = true`).
3. **Given** que já existe usuário com o e-mail informado **when** o administrador tenta cadastrar **then** o sistema recusa com "E-mail já cadastrado".
4. **Given** que já existe usuário com o login informado **when** o administrador tenta cadastrar **then** o sistema recusa com "Usuário já cadastrado".
5. **Given** uma senha que não atende à política configurada **when** o administrador tenta cadastrar **then** o sistema recusa e retorna as validações da política de senha.
6. **Given** um usuário existente **when** o administrador altera os dados básicos **then** o sistema atualiza os dados, atualiza as roles informadas e retorna os dados atualizados (**sem** alterar senha).
7. **Given** que não existe usuário com o identificador informado **when** o administrador solicita a edição **then** o sistema recusa com "Usuário não encontrado".
8. **Given** um usuário ativo **when** o administrador altera o status para inativo **then** o sistema marca o usuário como inativo **e** ele não consegue mais realizar login.
9. **Given** qualquer endpoint desta feature **when** chamado sem autenticação ou sem a policy `Users.Manage` **then** o sistema responde 401/403 e não executa a operação.
10. **Given** uma role informada que **não** existe no sistema **when** o administrador cadastra ou edita o usuário **then** o sistema recusa com erro de **Validation** ("Perfil informado não existe") e **não** cria/altera o usuário.
11. **Given** qualquer endpoint desta feature **when** chamado com entrada inválida **then** o sistema responde com erro de validação descrevendo os campos.
12. **Given** qualquer operação **when** executada **then** registra logs estruturados **sem** expor senha ou dados sensíveis.

## Requisitos não funcionais
- **Performance:** cadastro ≤ 500 ms; edição ≤ 300 ms (condições normais, sem contar e-mail). Consultas por e-mail e login usam índices adequados.
- **Confiabilidade:** criação do usuário + associação de roles é **transacional** (Unit of Work) — falha na associação de roles **não** deixa usuário criado parcialmente. Indisponibilidade do e-mail **não** falha o cadastro nesta história; a falha de preparação/envio é apenas registrada para acompanhamento futuro.
- **Segurança/Privacidade:** senhas nunca em texto puro (hash do Identity); senha temporária só em memória durante o fluxo de criação e **nunca** em log; apenas usuários autorizados cadastram/editam; validação de permissão por role/policy.
- **Dados:** usa as tabelas do ASP.NET Core Identity (já provisionadas pela AZ-12094); índices únicos de e-mail e login (normalizados) garantidos pelo Identity.
- **Observabilidade:**
  - **Logs** para: tentativa/sucesso/falha de cadastro, tentativa/sucesso/falha de edição, associação/alteração de roles, preparação de e-mail de boas-vindas.
  - **Métricas** para: usuários cadastrados, falhas de cadastro, usuários editados, falhas de edição, senhas temporárias geradas, e-mails de boas-vindas preparados.

## Endpoints sugeridos
> Contrato vindo da história (referência para `/approve-architecture`). Os DTOs reais vão para `Api/Contracts/Users/` com mappers `ToUseCase()` (ADR-0028).

### Cadastrar usuário — `POST /api/users` (auth obrigatória, perfil Administrador/policy)
Request **com** senha:
```json
{ "name": "João Silva", "email": "joao.silva@empresa.com", "userName": "joao.silva", "password": "Senha@123", "roles": ["Administrador"], "isActive": true }
```
Request **sem** senha:
```json
{ "name": "João Silva", "email": "joao.silva@empresa.com", "userName": "joao.silva", "roles": ["Operador"], "isActive": true }
```
Response (201 Created):
```json
{ "id": "user-id", "name": "João Silva", "email": "joao.silva@empresa.com", "userName": "joao.silva", "roles": ["Operador"], "isActive": true, "temporaryPasswordGenerated": true }
```

### Editar usuário — `PUT /api/users/{id}` (auth obrigatória, perfil Administrador/policy)
Request:
```json
{ "name": "João Silva", "email": "joao.silva@empresa.com", "userName": "joao.silva", "roles": ["Supervisor"], "isActive": true }
```
Response (200 OK):
```json
{ "id": "user-id", "name": "João Silva", "email": "joao.silva@empresa.com", "userName": "joao.silva", "roles": ["Supervisor"], "isActive": true }
```

### Interface de envio de e-mail (porta na Application) — **decidido**
Porta **dedicada** `IUserWelcomeEmailSender.SendWelcomeEmailAsync(email, userName, temporaryPassword, ct)` na Application,
com implementação temporária na Infrastructure que **apenas registra log** (sem expor a senha) e contém um `TODO` para a
história futura concluir o envio real. Mantém a `IEmailSender` genérica (AZ-12094) separada do contrato específico de boas-vindas.

## Dependências
- ASP.NET Core Identity + tabelas do Identity (já criadas na AZ-12094).
- Banco de dados da aplicação (SQL Server) + Unit of Work transacional.
- Serilog (logs estruturados) e OpenTelemetry (métricas).
- Serviço/interface de envio de e-mail (boas-vindas) — implementação real em história futura.
- Provedor de configuração segura para secrets; middleware de autorização; roles/policies do sistema.

## Premissas (confirmadas no refinamento)
- Roles informadas **já existem**; role inexistente → erro de **Validation** (feature não cria roles).
- Status ativo/inativo = flag **`IsActive`** no `ApplicationUser`; gate de login (AZ-12094) recusa inativos.
- **`Name`** adicionado ao `ApplicationUser` (perfil); o Identity padrão não possui esse atributo.
- Autorização por **policy nomeada `Users.Manage`** (não role literal no controller).
- E-mail de boas-vindas via **porta dedicada `IUserWelcomeEmailSender`** (impl. temporária com `TODO`).

## Riscos
- **Atomicidade usuário + roles:** criação envolve duas operações no Identity — garantir escopo transacional (Unit of Work) para não criar usuário parcial em caso de falha na associação de roles.
- **Vazamento de senha temporária:** mantida só em memória e fora de logs; o response apenas sinaliza `temporaryPasswordGenerated`.
- **Alteração de e-mail/login na edição:** trocar e-mail/login reabre o risco de colisão de unicidade e re-normalização no Identity — a edição deve revalidar unicidade contra **outros** usuários.
- **Impacto na AZ-12094:** o gate de `IsActive` no login altera o fluxo de autenticação já entregue — exige teste de regressão do login.

## Questões em aberto
- Nenhuma bloqueante. (As 4 decisões de design foram resolvidas no refinamento de 2026-06-25.)
- A confirmar com produto, **sem bloquear arquitetura:** mensagens exatas de erro (i18n) e se "Operador/Supervisor" são as roles iniciais do seed.

## Tasks existentes no tracker
Nenhuma task-filha cadastrada para o AZ-12114 no momento do import. As atividades serão derivadas no
`/approve-architecture` e poderão ser escritas de volta com `/sync-tasks`.

## Histórico de refinamento
- **2026-06-25** — Refinamento (`/brainstorm-story`). História já chegou rica do tracker (regras, NFRs, BDD). 4 decisões de design fechadas com o usuário:
  1. **Autorização** → policy nomeada `Users.Manage` (não role literal).
  2. **E-mail de boas-vindas** → porta dedicada `IUserWelcomeEmailSender` (impl. temporária com `TODO`).
  3. **Ativo/inativo** → flag `IsActive` no `ApplicationUser` + gate no login.
  4. **Role inexistente** → recusar com erro de Validation.
  Critérios de aceite ampliados (authz 401/403, role inexistente, validação de entrada) e seção de Regras de negócio adicionada. Status: **Pronta para arquitetura**.

---
> Próximo: execute `/approve-architecture` → produz `docs/architecture/AZ-12114-cadastro-e-edicao-de-usuario.md`.
