# Integração: E-mail (`IEmailSender`)

Porta: `IEmailSender` (Application). Seleção: `Integrations:Email:Provider`. Adapters em
`Infrastructure/Integrations/Email/<Provider>`. Segredos só em variável de ambiente / secret store.

## Provedores suportados
| Provedor | Quando usar | Prós | Contras | Config principal | Secret |
|---|---|---|---|---|---|
| **SMTP** | Dev, on-premise, servidor SMTP próprio | Universal, sem vendor; ótimo em dev (MailHog/Papercut) | Entregabilidade/escala por sua conta | `Host`, `Port`, `From` | usuário/senha SMTP |
| **SendGrid** | Volume alto, marketing + transacional | Escala, métricas, templates | Custo; conta/domínio verificado | `From` | `SENDGRID_API_KEY` |
| **Postmark** | Transacional puro (alta entregabilidade) | Entregabilidade excelente, simples | Não foca marketing | `From`, `MessageStream` | `POSTMARK_TOKEN` |
| **Amazon SES** | Já em AWS, custo baixo em escala | Barato, integra com AWS | Setup/saída do sandbox; menos features | `Region`, `From` | credenciais AWS |
| **Mailgun** | Transacional + APIs de validação | Boa API, roteamento | Custo; domínio verificado | `Domain`, `From` | `MAILGUN_API_KEY` |

## Recomendação (decisão dos agentes)
- **Dev / sem requisito de escala:** `SMTP` (MailHog/Papercut local).
- **Transacional com alta entregabilidade:** `Postmark` (ou `SendGrid` se também houver marketing).
- **Stack AWS / custo:** `Amazon SES`.
> O `solution-architect` registra a escolha (e o porquê) na arquitetura/ADR da feature.

## Contrato da porta
```csharp
public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string? From = null);
```
> Retorna `Result` (ver [error-handling.md](../../standards/error-handling.md)); falha de envio é tratada
> como erro de negócio/observável, não exceção solta.

## Notas
- Validar e-mail de destino; não logar conteúdo sensível/PII.
- Envolver chamadas no pipeline Polly e emitir métrica de envios/falhas.
- Templates de e-mail versionados; idioma conforme o usuário.

Adicione/troque o provedor com `/create-integration email <provider>`.
