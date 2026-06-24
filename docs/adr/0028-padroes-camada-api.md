# ADR-0028: Padrões da camada de API — estilo plugável, composição enxuta, SRP e status codes

- **Status:** Aceita
- **Data:** 2026-06-24
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
A camada `Api` vinha crescendo de forma desorganizada: `Program.cs` acumulando todo o registro/pipeline,
endpoints minimais com responsabilidades misturadas e status codes inconsistentes (tudo 200). Precisamos de
padrões vinculantes para manter o código **limpo, organizado e SOLID**, e para que **novas implementações já
nasçam certas** (skills/agents) e o **gate** barre o que fugir.

## Decisão
1. **Estilo de API plugável — Controllers por padrão, Minimal opcional.** O `/create-project` ganha a opção
   `api: controllers | minimal` (**default `controllers`**). Controllers usam `[ApiController]` + roteamento por
   atributo + `ControllerBase`; Minimal usa `MapGroup`/`Map*` por feature. Ver
   [`docs/standards/api-layer.md`](../standards/api-layer.md).
2. **SRP em toda a camada (e no resto do código).** Cada classe tem **uma** responsabilidade: controller/endpoint é
   **fino** (desserializa → despacha `IUseCaseDispatcher` → mapeia `Result`→envelope; **sem regra de negócio**);
   um handler = **um** caso de uso; serviços/adapters focados; mappers estáticos. Sem classes "faz-tudo".
   **Contratos HTTP (request/response) ficam em arquivos próprios** sob `Api/Contracts/<Recurso>/`, **nunca**
   aninhados no controller; o request expõe só os campos do cliente e um `ToUseCase(...)` que mapeia para o caso de uso.
3. **`Program.cs` enxuto via extension methods.** O registro de serviços e o pipeline ficam em extensões
   agrupadas por preocupação (`AddObservability`, `AddApiDocumentation`, `AddJwtAuthentication`,
   `AddRateLimiting`, `AddAuthorizationPolicies`, `UseApiPipeline`, `MapApiEndpoints`/`MapControllers`). O
   `Program.cs` apenas orquestra (poucas linhas).
4. **Status codes canônicos** ([`docs/standards/http-status-codes.md`](../standards/http-status-codes.md)):
   `200` (leitura/edição com corpo), **`201 Created` + `Location`** (criação de recurso), `204` (sem corpo),
   `400` validação, `401` não autenticado, `403` sem permissão, `404` não encontrado, `409` conflito,
   `429` rate limit, `500` inesperado. O envelope/`ToApiResult` mapeia `Result.ErrorType` → status e permite
   definir o status de sucesso (ex.: 201 no create).

## Consequências
- (+) Código mais legível, testável e coeso; `Program.cs` pequeno e navegável.
- (+) Respostas HTTP semânticas e consistentes para os consumidores da API.
- (+) Skills/agents passam a gerar e **revisar** nesse padrão; o gate (`validate-api-conventions.ps1`) barra desvios.
- (+) Times podem escolher Controllers (familiar) ou Minimal, sem mudar as demais camadas.
- (−) Refatorar a `Api` existente (Plataforma2A.Auth) tem custo pontual.
- (−) Parte do SRP é julgamento humano/IA (revisão), não 100% scriptável — o gate cobre o objetivo (Program enxuto, controllers finos).

## Alternativas consideradas
- **Manter só Minimal API:** menos cerimônia, mas o time prefere Controllers como default (organização/familiaridade). Vira opção.
- **Deixar `Program.cs` monolítico:** simples no início, mas degrada rápido. Rejeitada em favor das extensions.
- **Sucesso sempre 200:** simples, mas perde semântica (criação, sem-conteúdo). Rejeitada.

## Referências
- [`docs/standards/api-layer.md`](../standards/api-layer.md) · [`docs/standards/http-status-codes.md`](../standards/http-status-codes.md)
- [ADR-0014 — Result/Notification + envelope](0014-error-handling-result-notification.md) · [ADR-0002 — Clean Architecture](0002-clean-architecture.md)
- `scripts/validate-api-conventions.ps1` (gate)
