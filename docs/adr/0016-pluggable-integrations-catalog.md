# ADR-0016: Integrações plugáveis + catálogo em docs/integrations

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Serviços externos (e-mail, SMS, storage, pagamentos, etc.) têm vários provedores concorrentes
(ex.: e-mail → SMTP, SendGrid, Postmark, SES, Mailgun). A escolha deve ser informada e não pode acoplar a
Application a um provedor.

## Decisão
1. **Padrão de integração plugável** (igual filas/banco): cada categoria define uma **porta** na Application
   (ex.: `IEmailSender`) e **adapters** por provedor na Infrastructure, selecionados por configuração.
2. **Catálogo em `docs/integrations/<categoria>/`**: cada categoria documenta os provedores suportados,
   com prós/contras, custo, requisitos, config e secrets — para decisão consciente.
3. **Os agentes/skills consultam o catálogo**: `solution-architect` e `backend-developer` leem
   `docs/integrations/<categoria>` ao desenhar/implementar para escolher o provedor adequado ao contexto.
Regras em [`docs/standards/integrations.md`](../standards/integrations.md).

## Consequências
- (+) Decisão de provedor documentada e rastreável; troca por configuração; Application agnóstica.
- (+) A IA decide com base no catálogo, não em achismo.
- (−) Manter o catálogo atualizado conforme novos provedores surgem.

## Alternativas consideradas
- Acoplar a um provedor por categoria: lock-in e retrabalho ao trocar.
- Decidir caso a caso sem catálogo: escolhas inconsistentes entre features.
