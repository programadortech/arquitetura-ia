# Integração: SMS / WhatsApp (`ISmsSender`)

Porta: `ISmsSender` (Application). Seleção: `Integrations:Sms:Provider`. Segredos via env/secret store.

## Provedores suportados
| Provedor | Quando usar | Prós | Contras | Secret |
|---|---|---|---|---|
| **Twilio** | Padrão de mercado, global, WhatsApp | Cobertura, docs, WhatsApp API | Custo | `TWILIO_SID` / `TWILIO_TOKEN` |
| **Amazon SNS** | Já em AWS, SMS simples | Barato, integra AWS | Sem WhatsApp; features limitadas | credenciais AWS |
| **Zenvia** | Brasil (SMS/WhatsApp local) | Foco BR, suporte local | Menos global | `ZENVIA_TOKEN` |
| **Infobip** | Omnichannel global | SMS/WhatsApp/RCS | Complexidade | `INFOBIP_API_KEY` |

## Recomendação
- **Brasil + WhatsApp:** Zenvia ou Twilio. **Global:** Twilio. **Só SMS em AWS:** SNS.

## Contrato
```csharp
public interface ISmsSender
{
    Task<Result> SendAsync(string toPhoneE164, string message, CancellationToken cancellationToken);
}
```
Adicione com `/create-integration sms <provider>`.
