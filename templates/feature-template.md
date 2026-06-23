# Feature: <Feature Name>

- **Status:** Rascunho | Pronta para arquitetura | Em andamento | Concluída
- **Responsável:** <nome>
- **Data:** <YYYY-MM-DD>
- **Relacionados:** <links para projeto / epics / tickets>

## Descrição do problema
<Qual problema de negócio isto resolve? Por que agora?>

## Contexto de negócio
<Informação de fundo que o leitor precisa para entender o valor.>

## Resultados / métricas de sucesso
- <Resultado mensurável 1>
- <Resultado mensurável 2>

## Escopo
**No escopo**
- <…>

**Fora do escopo**
- <…>

## Critérios de aceite (Given/When/Then)
1. **Given** <contexto> **when** <ação> **then** <resultado observável>.
2. **Given** … **when** … **then** …

## Requisitos não funcionais
- **Performance:** <metas de latência / throughput>
- **Confiabilidade:** <disponibilidade, expectativas de retry/idempotência>
- **Segurança/Privacidade:** <authz, tratamento de PII, auditoria>
- **Dados:** <retenção, volume, crescimento>
- **Observabilidade:** <o que precisa ser mensurável>

## Dependências
- <sistemas upstream/downstream, times, dados>

## Riscos e premissas
- <risco / mitigação>
- <premissa / como validar>

## Questões em aberto
- [ ] <pergunta> — padrão proposto: <…>

---
> Próximo: execute `/approve-architecture` → produz `docs/architecture/<feature>.md`.
