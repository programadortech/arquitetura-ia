# 2A Smart — Mapa do domínio e plano de micro-serviços

> Análise do sistema legado **2A Smart** (https://2asmart.com.br) feita por inspeção da aplicação logada
> (Playwright, somente leitura). Serve de **referência** para modernizar o produto em micro-serviços Clean
> Architecture (.NET) + front Angular, pela fábrica. Atualize conforme aprofundarmos cada módulo.

## O que é o produto
**SaaS multi-tenant de gestão para pequenos negócios** (CRM + ERP-lite), forte em **assistência técnica** e varejo.
Cada usuário acessa **várias contas/empresas** (seletor de contas) — **multi-tenant** é pilar. O serviço de identidade
já iniciado (`apps/Plataforma2ASmart.Auth`) é a base de auth **deste produto**.

## Tecnologia legada (atual)
- **Monólito ASP.NET MVC** (rotas tipo `/ServiceOrder/Edit/{id}`; listagens via **PartialViews HTML** por AJAX, ex.
  `/os/orders-servicos`). Atrás de **Cloudflare**, com **Google Analytics** e integração **WhatsApp**.
- **Não há API JSON / OpenAPI** — telas renderizadas no servidor. As APIs serão **definidas do zero** (modernização).

## Módulos → entidades (observado)
| Módulo | Função | Entidades / rotas legadas |
|---|---|---|
| **Identidade & Contas** | Login, perfis, multi-tenant, permissões | Usuário, **Conta/Empresa (tenant)**, Perfil/Role, Permissão · `/conta/sair`, `/perfil/meus-dados`, `/configuracoes/minha-conta` |
| **Contatos (CRM)** | ~1.127 contatos, por bairro/cidade/UF | Contato · `/contato/listagem-contatos` |
| **Conteúdos / WhatsApp** | Mensageria/WhatsApp, lembretes, agenda | Número WhatsApp, Conteúdo, Lembrete · `/gerenciar-whatsapp-param-telefones-contas`, `/admin/webhook` |
| **Ordem de Serviço (OS)** | Abertura→andamento→conclusão, PDF | **OS** (nº, cliente, valor total, entrega prevista, status), **Produtos** e **Serviços** da OS, **Marcas** · `/os/listar-os`, `/os/nova-os`, `/os/detalhes-os/{id}`, `/os/pdf-os/{id}`, `/ServiceOrder/Edit/{id}` |
| **Vendas / Caixa (POS)** | Venda avulsa, finalizar/cancelar, "Nova Venda" (F2) | Venda (Aberto/Finalizada/Cancelada), Itens, Caixa, Categorias, Método de pagamento · `/vendas/listar-vendas` |
| **Finanças** | Receita/despesa, a receber/pagar, categorias | Transação (tipo 1=receita/2=despesa; status), Categoria, Método de pagamento, Conta a pagar/receber · `/transacao/listagem-transacoes` |
| **Agendamento** | Agenda + Google Calendar | Agendamento · `/agendamento/listagem-agendamentos`, `/configuracoes/google-calendario` |
| **Relatórios** | Indicadores (vendas, transações, despesas/categoria) | (lê dos demais) |
| **Administração** | Permissões, Webhooks, **Backup**, Gerenciar menus | Permissão, Webhook, Menu · `/gerenciar-permissao/*`, `/gerenciar-menu`, `/admin/webhook` |

## RBAC (aterrissado na tela de Permissões)
- Permissões geridas em **2 dimensões**: **Por Perfil** (`/gerenciar-permissao/por-perfil`) e **Por Conta** (`/gerenciar-permissao/contas`).
- **Perfis (roles):** Administrador, Administrador Master, Usuário Padrão, Cadastro, Vendas.
- Permissões em **árvore = estrutura de menu**: **Módulo → Funcionalidade → Ação** (ex.: `Finanças → Transações → {Lançar, Listar}`; `OS → OS → {Novo, Listar}`). Ações típicas: Novo, Listar, Lançar, Visão geral.
- **Gerenciar Menus** torna a árvore (e portanto as permissões) configurável.

## Proposta de micro-serviços (bounded contexts)
Cada um = um produto da fábrica (`/create-project`), Clean Architecture, com `openapi.json` publicado (consumido pelo Angular):

1. **Identity** — ✅ iniciado (`Plataforma2ASmart.Auth`). **Estender:** Contas/Empresas (tenant), vínculo usuário↔conta, troca de conta (tenant no JWT), **Perfis + Permissões** (menu/feature/action) por perfil e por conta.
2. **CRM/Contatos** — contatos, endereços, segmentação.
3. **Catálogo** — produtos, serviços, marcas (compartilhado por OS e Vendas).
4. **Service Orders (OS)** — OS + itens (produtos/serviços), status, PDF.
5. **Sales/POS** — vendas, caixa, método de pagamento, finalização.
6. **Finance** — transações, categorias, contas a pagar/receber.
7. **Scheduling** — agendamentos + Google Calendar.
8. **Messaging (WhatsApp/Conteúdos)** — números, envio, webhooks (mensageria/fila).
9. **Reporting** — indicadores (BFF/projeções dos demais).

**Transversais:** multi-tenancy (resolução + isolamento), RBAC, webhooks, backup, observabilidade (já temos a base).

## Ordem de build recomendada
**Identity (multi-tenant + RBAC)** → CRM/Contatos → Catálogo → **OS** → Sales/POS → Finance → Scheduling → Messaging → Reporting.
O alicerce (tenant + permissões) destrava todos os demais.

## Como construímos
Sem espelhar o legado HTML: cada serviço define **API REST limpa** (OpenAPI publicado). Fluxo por feature:
`/create-feature` → `/brainstorm-story` → `/approve-architecture` → `/create-usecase` → `/create-tests` → PR.

---
> Próximo: fatiar a **extensão do Identity** (Contas/tenant + RBAC) em features e iniciar a primeira.
