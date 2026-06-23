# Catálogo de Integrações

Fonte de **decisão** para integrações externas. Cada categoria define uma **porta** (Application) e lista os
**provedores plugáveis** suportados, com prós/contras, custo, requisitos, config e secrets. Padrão:
[`../standards/integrations.md`](../standards/integrations.md). Decisões registradas via ADR.

> **Para os agentes (obrigatório):** ao desenhar (`solution-architect`) ou implementar (`backend-developer`)
> algo que precise de uma integração, **leia o README da categoria** abaixo e escolha o provedor adequado
> ao contexto do produto — não decida por achismo. Adicione/troque provedores com `/create-integration`.

## Categorias
| Categoria | Porta | Provedores | Doc |
|---|---|---|---|
| E-mail | `IEmailSender` | SMTP · SendGrid · Postmark · Amazon SES · Mailgun | [email/](email/README.md) |
| SMS / WhatsApp | `ISmsSender` | Twilio · Amazon SNS · Zenvia · Infobip | [sms/](sms/README.md) |
| Storage de arquivos | `IFileStorage` | Local · Amazon S3 · Azure Blob · Google Cloud Storage · MinIO | [storage/](storage/README.md) |
| Pagamentos | `IPaymentGateway` | Stripe · Pagar.me · Mercado Pago · Adyen | [payments/](payments/README.md) |

> Categorias novas seguem o mesmo formato. Cada porta vive em `src/<Produto>.Application/Ports/<Categoria>`;
> adapters em `src/<Produto>.Infrastructure/Integrations/<Categoria>/<Provider>`; seleção por
> `Integrations:<Categoria>:Provider`.
