# Caso de Uso: <Name>

- **Feature:** [`../features/<feature>.md`](../features/<feature>.md)
- **Tipo:** Command | Query
- **Autor:** <nome> · **Data:** <YYYY-MM-DD>

## Contrato
- **Request:** `<Name>Request` — campos: <…>
- **Response:** `<Name>Response` — campos: <…>
- **Regras de validação:** <regras de campo; o que é rejeitado>

## Comportamento
<Orquestração passo a passo que o handler executa.>

## Portas utilizadas (interfaces da Application)
| Porta | Propósito | Implementada por (Infrastructure) |
|---|---|---|
| `IXxxRepository` | <…> | `XxxRepository` (Oracle) |
| `IQueuePublisher` | publica `<Event>` | provedor selecionado |

## Efeitos colaterais
- **DB:** <leituras/escritas, fronteira de transação>
- **Mensageria:** <eventos publicados/consumidos>
- **Jobs:** <trabalho enfileirado>

## Resiliência e observabilidade
- **Pipeline(s) Polly:** <nomes>
- **Span:** `<Name>` — atributos: <…>
- **Eventos de log:** <template → propriedades → nível>
- **Métricas:** <nome → tipo>

## Tratamento de erros
| Condição | Resultado |
|---|---|
| Validação falha | <resposta de erro / exceção> |
| Não encontrado | <…> |
| Falha de dependência | <retry/breaker → exposto como …> |

## Testes (mapeados aos critérios de aceite)
- **Unit:** caminho feliz; cada falha de validação; <casos de borda>; caminhos de erro.
- **Integration:** <round-trips de adapter envolvidos>.

---
> Referência: [`../standards/usecase-dispatcher.md`](../standards/usecase-dispatcher.md). Implemente com `/create-usecase`.

## Esqueleto (ilustrativo)
```csharp
public sealed record <Name>Request(/* … */) : IUseCaseRequest<<Name>Response>;
public sealed record <Name>Response(/* … */);

public sealed class <Name>Handler : IUseCase<<Name>Request, <Name>Response>
{
    // inject ports only (Application interfaces)
    public async Task<<Name>Response> HandleAsync(<Name>Request request, CancellationToken cancellationToken)
    {
        // 1. validate  2. load/act on domain  3. persist/publish  4. map response
    }
}
```
