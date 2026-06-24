# Feature: Autenticação e Gerenciamento de Senha

- **Status:** Implementada — arquitetura aprovada, código + testes na branch `feature/12094-autenticacao-e-gerenciamento-de-senha`
- **Tipo:** Negócio
- **Item (tracker):** [AZ-12094](https://dev.azure.com/T-SystemsdoBrasil/Yamaha%20-%20Rollout/_workitems/edit/12094) · Product Backlog Item · estado: New
- **Produto:** Plataforma2A.Auth
- **Data:** 2026-06-23

> Importada de um PBI do Azure DevOps (sem itens pai/filho). Refinar com `/brainstorm-story` antes de
> `/approve-architecture`.

## Descrição do problema
Como **usuário do sistema**, quero realizar login, manter a sessão ativa com refresh token, alterar minha
senha quando logado e redefinir quando não estiver logado, para **acessar o sistema com segurança e
recuperar o acesso** caso esqueça a senha.

## Contexto de negócio
Autenticação é a base do Plataforma2A.Auth. Cobre login, renovação via refresh token, troca de senha
autenticada e recuperação por e-mail — com postura de segurança que não revela existência de e-mail e
protege contra abuso.

## Resultados / métricas de sucesso
- Login responde em ≤ 500 ms (p95) e renovação de token em ≤ 300 ms (p95).
- Métricas: logins, falhas de login, refresh tokens emitidos, redefinições solicitadas/concluídas.
- Zero vazamento de existência de e-mail no fluxo de recuperação.

## Escopo
**Dentro:** login (e-mail+senha via Identity), emissão de Access Token (JWT) + Refresh Token com expiração,
renovação com rotação do refresh, troca de senha (logado), solicitação e redefinição de senha (deslogado) por e-mail.
**Fora:** cadastro/auto-registro, MFA, login social/SSO (histórias futuras).

## Regras de negócio
- Usar **ASP.NET Core Identity** para usuários, senhas, roles e validação de credenciais.
- Login valida e-mail/usuário e senha; em sucesso gera **Access Token JWT** + **Refresh Token** + data de expiração.
- Access Token com claims mínimas: `userId`, nome, e-mail, roles.
- Refresh Token: armazenado seguro (hash), com expiração; só vale se ativo, não expirado e vinculado ao usuário.
  Na renovação, **invalida o anterior** e gera novo.
- Alteração de senha (logado): senha atual + nova + confirmação; nova segue a política do Identity.
- Redefinição (deslogado): solicita por e-mail → token do Identity enviado por e-mail. **Não revela** se o e-mail existe.
- Após redefinir, **invalidar os refresh tokens ativos** do usuário.

## Critérios de aceite (Given/When/Then)
1. **Login com sucesso** — credenciais válidas → autentica e retorna Access Token + Refresh Token + expiração.
2. **Login recusado por senha inválida** → "Usuário ou senha inválidos", sem tokens.
3. **Login recusado para usuário inexistente** → mesma mensagem, sem revelar se o e-mail existe.
4. **Renovação com refresh válido** → invalida o anterior, gera novos tokens.
5. **Renovação recusada por refresh expirado** → "Refresh token inválido ou expirado".
6. **Alteração de senha (logado)** → altera, registra evento, "Senha alterada com sucesso".
7. **Alteração recusada por senha atual inválida** → "Senha atual inválida".
8. **Solicitação de redefinição** → mensagem genérica; se existir, envia token por e-mail.
9. **Redefinição com token válido** → altera a senha, invalida refresh tokens, "Senha redefinida com sucesso".
10. **Redefinição recusada por token inválido/expirado** → "Token de redefinição inválido ou expirado".
11. **Bloqueio por excesso de tentativas (brute force)** — Dado várias tentativas de login falhas acima do
    limite, Quando chega nova tentativa, Então responde `429 Too Many Requests` e não processa a autenticação.

## Requisitos não funcionais
- **Segurança/PII:** senhas nunca em texto puro; JWT assinado com chave segura; refresh em hash; nada sensível
  em logs; rate limit/brute force nos endpoints públicos; reset não revela e-mail.
- **Performance:** login ≤ 500 ms; renovação ≤ 300 ms; índices em usuário e refresh token.
- **Confiabilidade:** geração de token transacional ao persistir refresh; falhas de e-mail registradas;
  tokens expirados/revogados nunca reutilizados.
- **Observabilidade:** logs estruturados (tentativa/sucesso/falha de login, refresh usado, troca/solicitação/
  conclusão de reset); métricas dos fluxos.

## Dependências / Integrações
- ASP.NET Core Identity · JWT Bearer · banco do produto (SQL Server) · **serviço de e-mail** (catálogo
  `docs/integrations/email`) · Serilog · OpenTelemetry · secret store · rate limit.

## Endpoints sugeridos (do work item)
```
POST /api/auth/login            { email, password } -> { accessToken, refreshToken, expiresAt }
POST /api/auth/refresh-token    { accessToken, refreshToken } -> { accessToken, refreshToken, expiresAt }
POST /api/auth/change-password  (autenticado) { currentPassword, newPassword, confirmNewPassword }
POST /api/auth/forgot-password  { email } -> { message }
POST /api/auth/reset-password   { email, token, newPassword, confirmNewPassword }
```

## Riscos e premissas
- **Premissa:** adicionar ASP.NET Core Identity + JWT à stack (não está no esqueleto) → registrar **ADR**.
- **Premissa:** provedor de e-mail decidido pelo catálogo `docs/integrations/email`.
- **Risco:** rotação/invalidação de refresh token exige cuidado transacional (usar `IUnitOfWork`).

## Decisões do refinamento (resolvidas)
- **Expiração:** Access Token **15 min**, Refresh Token **7 dias** (com rotação a cada renovação).
- **Rate limit / brute force:** **.NET Rate Limiting nativo** (middleware), por endpoint público (login/forgot/reset).
- **E-mail:** porta **`IEmailSender`** + adapter **SMTP** em dev (provedor real plugável pelo catálogo `docs/integrations/email`).
- **Identificador de login:** **e-mail** + senha.
- **Auditoria de refresh tokens:** histórico com expiração e revogação (entidade `RefreshToken`, persistida via `IUnitOfWork`).

## Notas para a arquitetura (próximo passo)
- Registrar **ADR** do projeto: adoção de **ASP.NET Core Identity + JWT**.
- Modelar entidade **`RefreshToken`** (token em hash, `UserId`, `ExpiresAt`, `RevokedAt`, `CreatedAt`, índices) no SQL Server.
- Ports: `IIdentityService`, `IJwtTokenGenerator`, `IRefreshTokenStore`, `IEmailSender`. Handlers retornam **`Result<T>`**; Api responde **envelope**.
- Rate Limiting do .NET nos endpoints `login`, `forgot-password`, `reset-password`.

## Histórico de refinamento
- **2026-06-23** — Brainstorm. Decididos: tokens 15min/7d; rate limit nativo; `IEmailSender`+SMTP dev; login por e-mail;
  auditoria de refresh tokens. Adicionado AC #11 (brute force). Marcadas as notas de arquitetura (Identity/JWT como ADR, `RefreshToken`).

---
> Próximo: **"abra arquitetura da feature AZ-12094"** → casos de uso, modelo de dados (SQL Server), ports, ADRs.
> Implementação seguirá os padrões do produto: Result/Notification + envelope, `IUnitOfWork`, mappers estáticos.
