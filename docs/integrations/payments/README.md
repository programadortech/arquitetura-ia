# Integração: Pagamentos (`IPaymentGateway`)

Porta: `IPaymentGateway` (Application). Seleção: `Integrations:Payments:Provider`. Segredos via env/secret store.

## Provedores suportados
| Provedor | Quando usar | Prós | Contras | Secret |
|---|---|---|---|---|
| **Stripe** | Global, cartões/assinaturas | DX excelente, features | Pix/boleto limitados no BR | `STRIPE_SECRET_KEY` |
| **Pagar.me** | Brasil (Pix/boleto/cartão) | Foco BR, meios locais | Menos global | `PAGARME_API_KEY` |
| **Mercado Pago** | Brasil/LatAm, Pix | Alcance LatAm, Pix nativo | DX variável | `MP_ACCESS_TOKEN` |
| **Adyen** | Enterprise global | Cobertura, antifraude | Complexidade/contrato | credenciais Adyen |

## Recomendação
- **Brasil (Pix/boleto):** Pagar.me ou Mercado Pago. **Global/cartão/assinatura:** Stripe. **Enterprise:** Adyen.

## Notas de segurança/compliance
- **Nunca** trafegar/armazenar PAN de cartão (PCI) — usar tokenização do provedor.
- Idempotência obrigatória em criação de cobrança; webhooks validados por assinatura.

## Contrato (exemplo)
```csharp
public interface IPaymentGateway
{
    Task<Result<PaymentResult>> CreateChargeAsync(ChargeRequest request, CancellationToken ct);
}
```
Adicione com `/create-integration payments <provider>`.
