# Padrão: Status codes HTTP

> Vinculante. Codifica parte do [ADR-0028](../adr/0028-padroes-camada-api.md). Complementa o
> [tratamento de erros](error-handling.md) (Result/Notification + envelope `ApiResponse`).

Todo endpoint responde no **envelope `ApiResponse`** e usa o **status code semântico** correto. O sucesso é
decidido pelo endpoint/controller; o erro vem do `Result.ErrorType` (mapeado em `ToApiResult`).

## Sucesso
| Caso | Status | Quando usar |
|---|---|---|
| Leitura / edição com corpo | **200 OK** | GET, PUT/PATCH que retornam o recurso atualizado. |
| **Criação de recurso** | **201 Created** | POST que cria um recurso. Inclua o header **`Location`** apontando para o recurso (`/api/users/{id}`). |
| Operação sem corpo de retorno | **204 No Content** | DELETE, ou comandos que não retornam dados. |
| Aceito para processamento assíncrono | 202 Accepted | quando o processamento é enfileirado (jobs/fila) e ainda não concluiu. |

## Erro (mapeado de `Result.ErrorType`)
| `ErrorType` | Status | Significado |
|---|---|---|
| `Validation` | **400 Bad Request** | entrada inválida / regra de validação / confirmação não confere. |
| (sem autenticação) | **401 Unauthorized** | falta token/credencial válida (autenticação). |
| `Unauthorized` | **401 Unauthorized** | credencial/sessão inválida (ex.: login, refresh, senha atual). |
| `Forbidden` | **403 Forbidden** | autenticado, mas **sem permissão** (policy/role). |
| `NotFound` | **404 Not Found** | recurso inexistente. |
| `Conflict` | **409 Conflict** | violação de unicidade / estado conflitante (ex.: e-mail/login duplicado). |
| (rate limit) | **429 Too Many Requests** | limite de requisições estourado (middleware de rate limit). |
| (exceção não tratada) | **500 Internal Server Error** | erro inesperado (via `GlobalExceptionHandler`/ProblemDetails). |

> Observação semântica: **401 = não sei quem você é** (autenticação); **403 = sei quem você é, mas não pode**
> (autorização). Não revele existência de recurso/usuário quando isso for risco de segurança (ex.: login/reset).

## Como aplicar (envelope)
- Sucesso padrão (200): `result.ToApiResult(HttpContext)`.
- **Criação (201 + Location):** `result.ToApiResult(HttpContext, StatusCodes.Status201Created, location: $"/api/users/{id}")`.
- **Sem conteúdo (204):** `result.ToApiResult(HttpContext, StatusCodes.Status204NoContent)`.
- Erros: **não** escolha o status na mão — retorne `Result.Failure(new Error(code, msg, ErrorType.X))` no handler;
  o `ToApiResult` traduz `ErrorType → status`. Assim a regra fica no domínio/aplicação, não na borda.

## Regras
- **Nunca** retorne 200 para erro de negócio (use o `ErrorType` correto).
- **Criação sempre 201** (com `Location`), não 200.
- Comandos sem retorno → **204**, não 200 com corpo vazio.
- O status final de cada endpoint é verificado pelo gate `scripts/validate-api-conventions.ps1` e na revisão (`/review-pr`).
