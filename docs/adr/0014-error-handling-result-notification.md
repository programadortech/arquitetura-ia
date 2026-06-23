# ADR-0014: Tratamento de erros — Result/Notification + Envelope + middleware global

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Lançar `throw` para toda falha (inclusive regras de negócio esperadas) polui o fluxo, dificulta o front
e mistura erro esperado com excepcional. Precisamos de respostas previsíveis e fáceis de integrar.

## Decisão
Adotar três peças complementares:
1. **Result/Notification** (Application): casos de uso retornam `Result<T>` que acumula `Error`
   (código + mensagem + tipo: Validation/NotFound/Conflict/Unauthorized) via um `Notification`.
   `throw` fica **apenas para o inesperado** (bugs, falhas de infra).
2. **Envelope padronizado** (Api): toda resposta sai como `ApiResponse<T>`
   `{ success, data, errors[], traceId, timestamp }`. `Result` é mapeado para o envelope + status HTTP
   (200/201, 400 validação, 404, 409, 401/403).
3. **Middleware global de exceções** (`IExceptionHandler` do .NET 10): exceções **não tratadas** caem
   nele, viram `ProblemDetails`/envelope com `traceId` e são logadas (sem vazar stack/PII ao cliente).

Regras completas em [`docs/standards/error-handling.md`](../standards/error-handling.md).

## Consequências
- (+) Falhas de negócio são dados (Result), não exceções — código mais limpo e testável.
- (+) Front integra com um contrato único (envelope) e correlaciona por `traceId`.
- (+) Erros inesperados têm um único ponto de tratamento.
- (−) Mais tipos base (`Result`, `Error`, `Notification`, `ApiResponse`) e disciplina de mapeamento.

## Alternativas consideradas
- Exceções para tudo: simples, mas custo de performance/clareza e respostas inconsistentes.
- Apenas envelope + exceções: padroniza a saída mas mantém o fluxo sujo (rejeitado como padrão; ver opção B no histórico).
