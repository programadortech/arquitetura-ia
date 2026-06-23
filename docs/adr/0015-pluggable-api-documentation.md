# ADR-0015: Documentação de API plugável (OpenAPI + Scalar/Swagger)

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
A API precisa de documentação navegável para o front e parceiros. Há várias UIs (Swagger UI, Scalar,
ReDoc) e queremos não acoplar a uma só.

## Decisão
Usar o **OpenAPI nativo do .NET 10** (`Microsoft.AspNetCore.OpenApi`) como fonte do documento e expor a
UI de forma **plugável**, selecionável no `/create-project` (`apidocs`):
- **Scalar** (`/scalar`) — UI moderna (default junto com Swagger).
- **Swagger UI** (Swashbuckle, `/swagger`) — clássico, amplamente conhecido.
- **ReDoc** — opção adicional.
Default do template: **Scalar + Swagger** (ambos ligados em Development). Em produção, a exposição da doc
é controlada por configuração. Regras em [`docs/standards/api-documentation.md`](../standards/api-documentation.md).

## Consequências
- (+) Documento OpenAPI único, várias UIs sem lock-in; bom DX para o front.
- (+) Selecionável por projeto.
- (−) Mais pacotes quando ambas as UIs são ligadas (overhead pequeno, só dev).

## Alternativas consideradas
- Só Swagger/Swashbuckle: difundido, porém UI datada e acoplamento.
- Sem documentação: inviável para integração com o front.
