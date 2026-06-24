# ADR-0027: Cadastro de usuário público (anônimo) com mitigação de escalada de privilégio

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead / PO), Claude
- **Feature:** [AZ-12114 — Cadastro e Edição de Usuário](../features/AZ-12114-cadastro-e-edicao-de-usuario.md)
- **Revisa:** a decisão de autorização de [ADR-0026](0026-gestao-administrativa-de-usuarios.md) (apenas para o `POST`).

## Contexto
A história AZ-12114 foi importada com o cadastro **restrito a administradores** (AC #9 + NFR de segurança), e o
[ADR-0026](0026-gestao-administrativa-de-usuarios.md) aplicou a policy `users:manage` em **ambos** os endpoints.

O PO decidiu que o **cadastro será aberto** (sem login) — `POST /api/users` público. Isso cria um risco direto:
se o endpoint anônimo aceitasse `roles[]`, **qualquer pessoa poderia criar uma conta com a role `Administrador`**
(escalada de privilégio); e `isActive` permitiria autoativar.

## Decisão
Vamos tornar **`POST /api/users` anônimo** (`AllowAnonymous`), com guardas no servidor:

1. O corpo público **não aceita `roles` nem `isActive`**. O servidor **fixa** a role padrão **`Usuario`** e `isActive = true`.
   Atribuição/elevação de perfis só pelo `PUT` (admin).
2. **`PUT /api/users/{id}` continua exigindo a policy `users:manage`** (role `Administrador`) — inalterado.
3. Demais regras do cadastro permanecem (senha opcional → temporária, e-mail de boas-vindas stub, unicidade,
   criação transacional).
4. **Sem rate limit** no cadastro público nesta entrega (decisão do PO) — risco de abuso/criação em massa
   **aceito e registrado**; recomenda-se reavaliar (rate limit por IP / CAPTCHA / confirmação de e-mail) se houver abuso.

## Consequências
- (+) Permite auto-cadastro sem login, como pedido.
- (+) Fecha a escalada de privilégio: anônimo nunca escolhe role nem ativa/eleva.
- (−) **Diverge do AC #9** e do NFR de segurança originais da história — divergência consciente, registrada aqui.
- (−) Endpoint aberto sem rate limit é exposto a abuso/criação em massa (risco aceito; mitigação recomendada como follow-up).
- (−) A role `Usuario` é criada automaticamente se não existir (comportamento do `UserAdminService`).

## Alternativas consideradas
- **Manter `users:manage` no cadastro (ADR-0026):** mais seguro, mas o PO quer o cadastro aberto. Não atende ao pedido.
- **Público aceitando `roles[]`:** atende ao pedido literal, mas abre escalada de privilégio (qualquer um vira admin). Rejeitada.
- **Público + rate limit por IP:** recomendado, mas o PO optou por não aplicar agora. Fica como follow-up.

## Referências
- [ADR-0026 — Gestão administrativa de usuários](0026-gestao-administrativa-de-usuarios.md)
- [AZ-12114 (feature)](../features/AZ-12114-cadastro-e-edicao-de-usuario.md) · [arquitetura](../architecture/AZ-12114-cadastro-e-edicao-de-usuario.md)
