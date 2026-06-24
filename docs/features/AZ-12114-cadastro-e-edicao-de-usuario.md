# Feature: Cadastro e Edição de Usuário

- **Status:** Pronta para arquitetura (importada do tracker — refinar com `/brainstorm-story` se desejar)
- **Tipo:** Negócio
- **Item (tracker):** [AZ-12114](https://dev.azure.com/T-SystemsdoBrasil/Yamaha%20-%20Rollout/_workitems/edit/12114) · Product Backlog Item · estado: New
- **Sprint:** Yamaha - Rollout\Sprint 16
- **Produto:** Plataforma2A.Auth
- **Data:** 2026-06-24
- **Relacionados:** depende de autenticação/Identity já entregue em [AZ-12094](AZ-12094-autenticacao-e-gerenciamento-de-senha.md).

## Descrição do problema
Como **administrador do sistema**, quero **cadastrar e editar usuários** utilizando ASP.NET Core Identity,
para permitir o **controle de acesso ao sistema** de forma segura e padronizada.

## Contexto de negócio
O Plataforma2A.Auth já cobre autenticação e gerenciamento de senha (AZ-12094). Esta história adiciona a
**administração de usuários** (CRUD de cadastro/edição + perfis), base para o controle de acesso por roles.
O **envio real de e-mail de boas-vindas não faz parte** desta história — apenas a **interface** preparada,
com a implementação marcada como `TODO` para uma história futura.

## Resultados / métricas de sucesso
- Cadastro de usuário responde em ≤ 500 ms (p95), desconsiderando envio de e-mail.
- Edição de usuário responde em ≤ 300 ms (p95).
- Zero usuário criado parcialmente (criação + roles é transacional).
- Métricas: usuários cadastrados, falhas de cadastro, usuários editados, falhas de edição, senhas temporárias geradas, e-mails de boas-vindas preparados.

## Escopo
**No escopo**
- Cadastrar usuário via ASP.NET Core Identity informando: nome, e-mail, usuário/login, roles/perfis, status ativo/inativo, **senha opcional**.
- Senha informada → cria com a senha recebida. Senha ausente → **gera senha temporária** seguindo a política do Identity.
- Após cadastro sem senha informada, **preparar** o envio de e-mail de boas-vindas (login + senha temporária) via **interface** (`IUserWelcomeEmailSender`), com implementação `TODO`.
- Editar dados básicos: nome, e-mail, usuário/login, roles/perfis, status ativo/inativo.
- Atualizar vínculos de roles no Identity ao alterar perfis.
- Unicidade de e-mail e de usuário/login.
- Usuário inativo não autentica.
- Endpoints com autenticação + autorização (role/policy de administrador) e validação de entrada.

**Fora do escopo**
- **Envio real de e-mail** (só a interface + TODO).
- **Alteração de senha** (continua nos fluxos específicos de senha — AZ-12094); a edição **não** altera senha.
- Auto-registro/cadastro público, MFA, SSO.

## Critérios de aceite (Given/When/Then)
1. **Cadastro com senha informada** — **Dado** um administrador autenticado **e** dados obrigatórios + senha válida, **quando** solicita o cadastro, **então** o sistema cria o usuário no Identity, associa as roles, retorna os dados e **não** gera senha temporária.
2. **Cadastro sem senha informada** — **Dado** um admin autenticado **e** dados obrigatórios **sem** senha, **quando** solicita o cadastro, **então** o sistema **gera senha temporária** válida, cria o usuário, associa as roles, **prepara** o e-mail de boas-vindas e retorna os dados.
3. **Cadastro recusado por e-mail duplicado** — **Dado** já existir usuário com o e-mail, **quando** solicita o cadastro, **então** recusa com "E-mail já cadastrado".
4. **Cadastro recusado por usuário duplicado** — **Dado** já existir o nome de usuário, **quando** solicita o cadastro, **então** recusa com "Usuário já cadastrado".
5. **Cadastro recusado por senha fora da política** — **Dado** senha que não atende à política, **quando** solicita o cadastro, **então** recusa e retorna as validações da política.
6. **Edição com sucesso** — **Dado** um usuário existente, **quando** altera os dados básicos, **então** atualiza os dados e as roles e retorna os dados atualizados (**sem** alterar senha).
7. **Edição recusada para usuário inexistente** — **Dado** id inexistente, **quando** solicita a edição, **então** recusa com "Usuário não encontrado".
8. **Inativação de usuário** — **Dado** um usuário ativo, **quando** altera o status para inativo, **então** o usuário fica inativo **e não consegue autenticar**.
9. **Autorização** — **Dado** um chamador sem permissão de administrador, **quando** acessa cadastrar/editar, **então** a operação é negada (401/403).
10. **Observabilidade** — **Dado** qualquer operação de cadastro/edição, **quando** executada, **então** são emitidos logs estruturados (tentativa/sucesso/falha, associação de roles, preparação de e-mail) **sem** expor senha/PII.

## Requisitos não funcionais
- **Segurança/PII:** senhas nunca em texto puro; senha temporária só em memória durante a criação; **nunca** logar senha temporária nem PII; endpoints exigem autenticação e autorização por role/policy; validação de entrada em todos os endpoints.
- **Performance:** cadastro ≤ 500 ms; edição ≤ 300 ms; consultas por e-mail/usuário com índices adequados.
- **Confiabilidade:** criação do usuário + associação de roles **transacional** (falha em roles ⇒ usuário não fica criado parcialmente); indisponibilidade do e-mail **não** falha o cadastro — a falha de envio é apenas registrada para acompanhamento futuro.
- **Observabilidade:** logs estruturados (Serilog) e métricas (OpenTelemetry) para os marcos listados nos resultados.

## Dependências / Integrações
- ASP.NET Core Identity · banco do produto (SQL Server) · Serilog · OpenTelemetry · middleware de autorização (roles/policies) · **interface** de envio de e-mail (`IUserWelcomeEmailSender`) · secret store.

## Endpoints sugeridos (do work item)
```
POST /api/users        (admin)  { name, email, userName, password?, roles[], isActive } -> usuário criado (+ temporaryPasswordGenerated)
PUT  /api/users/{id}   (admin)  { name, email, userName, roles[], isActive }            -> usuário atualizado (não altera senha)
```

## Interface de e-mail (preparada — envio real é história futura)
```csharp
public interface IUserWelcomeEmailSender
{
    Task SendWelcomeEmailAsync(string email, string userName, string temporaryPassword, CancellationToken cancellationToken = default);
}
// Implementação temporária: apenas loga "envio preparado" (sem a senha temporária) + // TODO: envio real.
```
> Avaliar na arquitetura se reaproveita a porta `IEmailSender` já existente (AZ-12094) ou cria `IUserWelcomeEmailSender` dedicada conforme o work item.

## Riscos e premissas
- **Premissa:** roles/policies de "Administrador" e equivalentes já existem (ou serão criadas) — confirmar na arquitetura.
- **Risco:** atomicidade criação + roles exige `IUnitOfWork`/transação (mesmo cuidado do refresh token em AZ-12094).
- **Premissa:** "senha opcional" + senha temporária seguem a política de senha do Identity já configurada.

## Questões em aberto
- [ ] Reusar `IEmailSender` (AZ-12094) ou criar `IUserWelcomeEmailSender`? — proposta: criar a dedicada (contrato do work item) e, se fizer sentido, implementá-la sobre o `IEmailSender` existente.
- [ ] Endpoints de **listar/consultar** e **excluir** usuário fazem parte? — o work item só cita POST/PUT; assumir **fora do escopo** salvo confirmação.
- [ ] Política de autorização: role fixa "Administrador" vs policy nomeada — confirmar com o time.

---
> Próximo: **"faça um brainstorm da história AZ-12114"** (refinar as questões em aberto) e, em seguida,
> **"abra arquitetura da feature AZ-12114"** → casos de uso, modelo de dados, ports, ADRs.
