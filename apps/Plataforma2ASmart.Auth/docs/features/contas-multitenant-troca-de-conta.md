# Feature: Contas/Empresas (multi-tenant) + troca de conta

- **Status:** Rascunho (pronta para brainstorm)
- **Responsável:** —
- **Data:** 2026-06-27
- **Relacionados:**
  - Produto: [Plataforma2ASmart.Auth](../PRODUCT.md) · Predecessoras: AZ-12094 (Identity+JWT), AZ-12114 (Cadastro de usuário)
  - Análise do produto: [`docs/platform/2asmart-domain-map.md`](../../../docs/platform/2asmart-domain-map.md)
  - Próximas: **Perfis + Permissões (RBAC)** e **Permissões por conta + Menus** (dependem desta)

## Descrição do problema
O 2A Smart é **multi-tenant**: um usuário acessa **várias contas/empresas** e tudo (dados, permissões, indicadores)
é **escopado pela conta ativa**. Hoje o `Plataforma2ASmart.Auth` tem **usuários e roles globais**, mas **não existe o
conceito de Conta (tenant)** nem o vínculo usuário↔conta nem a troca de conta. Sem esse alicerce, nenhum micro-serviço
de negócio (OS, Finanças, Vendas…) consegue isolar dados por empresa, e o RBAC por conta não tem onde se apoiar.

## Contexto de negócio
No sistema legado, o cabeçalho traz um **seletor de contas** (ex.: PTECH Assistência Técnica, LCB Corretora, Capi
Medical, Grupo 2A, …) — o mesmo login enxerga **N empresas**. As permissões são geridas **"Por Perfil"** e **"Por
Conta"**. Logo: **o usuário é global** (uma identidade), e **a conta ativa define o contexto** de dados e de permissões.

## Resultados / métricas de sucesso
- Um usuário com várias contas **lista** suas contas e **troca** a conta ativa; o token reflete a conta ativa.
- **Zero vazamento entre tenants:** uma requisição na conta A nunca enxerga dados da conta B (meta absoluta).
- Todo dado de negócio (nos demais serviços) passa a ter **TenantId** e filtro global por conta ativa.
- Trocar de conta responde em ≤ 300 ms.

## Escopo
**No escopo**
- **Entidade Conta/Empresa (tenant):** `Id`, `Nome` (razão/fantasia), `Documento` (CNPJ/CPF, opcional), `Ativo`, `CriadoEm`.
- **Vínculo usuário↔conta (Membership):** um usuário pertence a **N contas**, com um **perfil (role) por conta**; estados (ativo/convidado/removido).
- **Criar conta:** ao criar, o usuário criador vira **proprietário/admin** daquela conta (membership inicial).
- **Listar minhas contas:** endpoint que retorna as contas do usuário autenticado (+ qual é a ativa).
- **Trocar conta ativa:** emite um **novo access token** com a **claim de tenant (conta ativa)** e o perfil daquela conta; exige membership válido.
- **Tenant na sessão:** o access token (AZ-12094) passa a carregar `tenant` (conta ativa); login retorna as contas + define a conta ativa default.
- **Fundação de isolamento:** padrão **banco único + coluna `TenantId`** com **filtro global por tenant** (a ser aplicado nos serviços de negócio); contrato de **resolução de tenant** a partir do token.
- API REST limpa + **OpenAPI publicado** (consumido pelo Angular). **Não** espelhar o HTML legado.

**Fora do escopo**
- Árvore de **permissões/menus** e atribuição (Features B e C — RBAC).
- Convite/onboarding de usuários para uma conta por e-mail (pode virar feature própria; aqui só o vínculo).
- Planos/cobrança/limites por conta (billing).
- Administração visual de contas (telas) — isto é back-end/API.

## Premissas confirmadas
- **Usuário é global** (um login); a **conta ativa** dá o contexto. Vínculo via Membership (usuário↔conta↔perfil).
- **Multi-tenancy = banco único + `TenantId`** + filtro global (decisão de arquitetura; formalizar em ADR).

## Critérios de aceite (Given/When/Then)
1. **Given** um usuário autenticado com 2+ contas **when** chama `GET /api/accounts` (minhas contas) **then** recebe a lista das contas a que pertence e a indicação de qual é a **ativa**.
2. **Given** o login bem-sucedido **when** o usuário se autentica **then** o access token carrega a **conta ativa** (claim `tenant`) e o **perfil** dessa conta; se houver mais de uma, usa a **default** (última usada ou a primeira).
3. **Given** um usuário com membership na conta B **when** chama `POST /api/accounts/{id}/switch` **then** recebe um **novo access token** com `tenant=B` e o perfil de B (refresh mantém a identidade).
4. **Given** um usuário **sem** membership na conta X **when** tenta trocar para X **then** recebe **403** e o token não muda.
5. **Given** uma requisição autenticada com `tenant=A` **when** acessa dados de negócio **then** só enxerga dados de A (filtro global por `TenantId`); dados de B nunca aparecem.
6. **Given** um usuário autenticado **when** chama `POST /api/accounts` para criar uma conta **then** a conta é criada e o usuário vira **proprietário/admin** dela (membership inicial), retornando 201 + Location.
7. **Given** um usuário em várias contas **when** olha o token após troca **then** as **roles/claims** refletem o perfil **da conta ativa** (não de outra).
8. **Given** qualquer operação de conta/membership **when** executada **then** registra log estruturado **sem** dados sensíveis, com `userId` e `tenantId`.

## Requisitos não funcionais
- **Segurança (crítico):** **isolamento de tenant é inviolável** — nenhuma rota de negócio retorna dados fora da conta ativa. A claim `tenant` é **assinada no JWT**; trocar de conta exige membership válido (verificado no servidor, nunca confiando no cliente). Sem IDOR entre contas.
- **Performance:** trocar de conta ≤ 300 ms; `GET /api/accounts` ≤ 200 ms. `TenantId` **indexado** em toda tabela de negócio.
- **Auditabilidade:** registrar criação de conta, vínculo/desvínculo e troca de conta (quem, quando, qual conta).
- **Dados:** `TenantId` obrigatório nas tabelas de negócio dos demais serviços; tabelas de Identity (usuário) permanecem globais; Membership liga usuário↔conta↔perfil.
- **Observabilidade:** propagar `tenantId` em logs/traces (enriquecer o contexto).

## Dependências
- AZ-12094 (Identity + JWT) — o `JwtTokenGenerator` e a validação passam a incluir a claim `tenant`.
- AZ-12114 (usuários) — o usuário existe; ganha memberships.
- BuildingBlocks; contrato de **resolução de tenant** (a ser usado por todos os micro-serviços de negócio).
- ADR-0032 (publicação do contrato OpenAPI) — o novo contrato sai publicado.

## Riscos e premissas
- **Risco #1 — vazamento entre tenants:** mitigar com filtro global obrigatório por `TenantId` + testes de isolamento; revisão de segurança no merge.
- **Risco — confiar no cliente para o tenant:** o `tenant` vem **do token** (assinado), e a troca **revalida membership** no servidor.
- **Risco — sessão e troca:** definir se a troca **rotaciona** o refresh (cookie httpOnly — ADR-P0003) ou só re-emite o access. *A confirmar no brainstorm.*
- **Premissa:** a conta ativa default = última usada (persistida) ou a primeira; *a confirmar.*

## Questões em aberto
- [ ] Trocar de conta: re-emitir só o **access token** (recomendado) ou também rotacionar o refresh?
- [ ] O refresh token (cookie) é **por identidade** (independe da conta) — confirmar que a troca não exige novo login.
- [ ] Conta tem **proprietário único** + admins, ou só perfis? (provável: dono + perfis por conta — alinha com RBAC).
- [ ] `Documento` (CNPJ) é obrigatório/único por conta?
- [ ] Auto-criação de conta no cadastro do 1º usuário (bootstrap) vs criação explícita?

---
> Próximo: `/brainstorm-story` desta feature (refinar as questões acima) → `/approve-architecture` (modelo de dados `TenantId`, claim `tenant`, filtro global, ADR de multi-tenancy).
