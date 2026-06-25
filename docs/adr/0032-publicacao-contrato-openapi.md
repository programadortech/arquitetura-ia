# ADR-0032: Publicação automática do contrato OpenAPI (build-time) versionado em `contracts/`

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude

## Contexto
A fábrica de **front-end** (repo separado — ADR-0031) consome o back-end por um **client TypeScript gerado a partir
do OpenAPI**. Para isso, cada produto precisa publicar seu contrato de forma **estável, automática e versionada** —
sem depender da API no ar. O OpenAPI já existe em runtime (`/openapi/v1.json`, ADR-0015), mas o front precisa de um
**arquivo** consumível e rastreável.

## Decisão
Vamos **gerar o documento OpenAPI em build time** (sem subir o servidor) e **commitá-lo** no repositório, por produto:
- Pacote **`Microsoft.Extensions.ApiDescription.Server`** no projeto `*.Api`; geração **sob demanda** via
  `scripts/generate-openapi.ps1` (não em todo build, para não poluir o working tree).
- Caminho canônico **`apps/<Produto>/contracts/openapi.json`** (versionado; diff visível no PR).
- A geração constrói o host só para extrair o documento; o `Program` **pula o seed** quando
  `OPENAPI_GENERATION=true` (sem banco/segredo real — usa valores dummy).
- **Gate de CI:** o job de produto regenera e roda `git diff --exit-code` no contrato — **falha se estiver
  desatualizado**. Assim o arquivo é sempre o espelho da API.
- **EOL fixo (LF)** do contrato via `.gitattributes` para o gate não acusar diferença Windows×Linux.
- **Consumo pelo front:** raw URL do GitHub fixada por branch/tag, ex.:
  `https://raw.githubusercontent.com/programadortech/arquitetura-ia/main/apps/<Produto>/contracts/openapi.json`.

## Consequências
- (+) Contrato sempre atualizado e versionado; o front gera o client tipado a partir dele (ADR-0007 no repo de FE).
- (+) Não exige a API no ar nem banco para gerar.
- (−) Passo extra no CI (regenera + diff) e a disciplina de commitar o `openapi.json` quando a API muda (imposto pelo gate).
- (−) Side-effects de startup que tocam banco precisam ser guardados em modo de geração (feito para o seed de roles).

## Alternativas consideradas
- **Artefato/Release do CI:** versiona por release, mas o arquivo não fica visível no repo (menos diff/PR).
- **Endpoint vivo `/openapi/v1.json`:** sempre atual, mas exige API no ar e acesso de rede; não reprodutível offline.

## Referências
- ADR-0031 (fábrica de front-end) · ADR-0015 (OpenAPI) · `scripts/generate-openapi.ps1` · ADR-0007 no repo `arquitetura-ia-frontend`.
