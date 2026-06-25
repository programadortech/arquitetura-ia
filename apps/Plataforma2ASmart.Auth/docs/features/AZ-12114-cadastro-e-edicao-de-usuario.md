# Feature: Cadastro e Edição de Usuário

- **Status:** Pronta para arquitetura
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
- Validação de entrada, autenticação + autorização (Administrador/policy) em todos os endpoints.
- Logs estruturados (sem dados sensíveis) e métricas.

**Fora do escopo**
- Envio real do e-mail de boas-vindas (história futura — apenas preparar/registrar).
- Alteração de senha pela edição de usuário (senha é tratada apenas pelos fluxos de senha da AZ-12094).
- Exclusão de usuário, fluxo de listagem/paginação e tela/UI (frontend).
- Gestão de roles em si (criação de roles novas) além de associar as existentes ao usuário.

## Critérios de aceite (Given/When/Then)
1. **Given** um administrador autenticado **when** cadastra um usuário com dados obrigatórios **e** uma senha válida **then** o sistema cria o usuário no Identity, associa as roles, retorna os dados do usuário e **não** gera senha temporária.
2. **Given** um administrador autenticado **when** cadastra um usuário com dados obrigatórios **sem** informar senha **then** o sistema gera uma senha temporária válida (política do Identity), cria o usuário, associa as roles, **prepara** o e-mail de boas-vindas e retorna os dados (`temporaryPasswordGenerated = true`).
3. **Given** que já existe usuário com o e-mail informado **when** o administrador tenta cadastrar **then** o sistema recusa com "E-mail já cadastrado".
4. **Given** que já existe usuário com o login informado **when** o administrador tenta cadastrar **then** o sistema recusa com "Usuário já cadastrado".
5. **Given** uma senha que não atende à política configurada **when** o administrador tenta cadastrar **then** o sistema recusa e retorna as validações da política de senha.
6. **Given** um usuário existente **when** o administrador altera os dados básicos **then** o sistema atualiza os dados, atualiza as roles informadas e retorna os dados atualizados (**sem** alterar senha).
7. **Given** que não existe usuário com o identificador informado **when** o administrador solicita a edição **then** o sistema recusa com "Usuário não encontrado".
8. **Given** um usuário ativo **when** o administrador altera o status para inativo **then** o sistema marca o usuário como inativo **e** ele não consegue mais realizar login.
9. **Given** qualquer endpoint desta feature **when** chamado **then** exige autenticação + autorização (Administrador/policy) e valida a entrada.
10. **Given** qualquer operação **when** executada **then** registra logs estruturados **sem** expor senha ou dados sensíveis.

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

### Interface de envio de e-mail (porta na Application)
A história sugere `IUserWelcomeEmailSender.SendWelcomeEmailAsync(email, userName, temporaryPassword, ct)`, com uma
implementação temporária na Infrastructure que **apenas registra log** (sem expor a senha) e contém um `TODO` para a
história futura concluir o envio real. *Nota de arquitetura:* o produto já possui a porta `IEmailSender` (AZ-12094);
em `/approve-architecture` decidir entre reutilizá-la ou introduzir a porta específica de boas-vindas.

## Dependências
- ASP.NET Core Identity + tabelas do Identity (já criadas na AZ-12094).
- Banco de dados da aplicação (SQL Server) + Unit of Work transacional.
- Serilog (logs estruturados) e OpenTelemetry (métricas).
- Serviço/interface de envio de e-mail (boas-vindas) — implementação real em história futura.
- Provedor de configuração segura para secrets; middleware de autorização; roles/policies do sistema.

## Riscos e premissas
- **Premissa:** as roles informadas no cadastro/edição **já existem** no sistema (a história não cria roles novas). *A confirmar* o comportamento quando uma role inexistente é informada — padrão proposto: recusar com erro de validação.
- **Premissa:** "Status ativo/inativo" será modelado como flag no `ApplicationUser` (ex.: `IsActive`), e o gate de login (inativo não autentica) será aplicado no fluxo de autenticação existente (AZ-12094). *A confirmar* na arquitetura.
- **Premissa:** "Nome" é um atributo de perfil adicionado ao `ApplicationUser` (o Identity padrão não tem `Name`). *A confirmar.*
- **Risco:** transação envolvendo criação de usuário + roles no Identity — garantir atomicidade via Unit of Work / escopo transacional para não criar usuário parcial.
- **Risco:** vazar senha temporária em log/response — mitigado mantendo-a só em memória e fora de logs; o response apenas sinaliza `temporaryPasswordGenerated`.

## Questões em aberto
- [ ] Reutilizar a porta `IEmailSender` existente ou criar `IUserWelcomeEmailSender` dedicada? — padrão proposto: porta dedicada de boas-vindas (alinha com o contrato da história), implementação temporária com `TODO`.
- [ ] Role inexistente no cadastro/edição → erro de validação (proposto) ou criar a role? — padrão proposto: **erro de validação**.
- [ ] Política de autorização: role fixa "Administrador" ou policy nomeada (ex.: `Users.Manage`)? — padrão proposto: **policy nomeada** mapeada para a role administrativa.

## Tasks existentes no tracker
Nenhuma task-filha cadastrada para o AZ-12114 no momento do import. As atividades serão derivadas no
`/brainstorm-story`/`/approve-architecture` e poderão ser escritas de volta com `/sync-tasks`.

---
> Próximo: execute `/approve-architecture` → produz `docs/architecture/AZ-12114-cadastro-e-edicao-de-usuario.md`.
