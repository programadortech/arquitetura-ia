---
name: create-feature
description: Define uma nova feature como um documento escopado e testável (problema, resultados, escopo, critérios de aceitação, NFRs) antes de qualquer design ou código. Use quando o usuário disser "crie uma feature Y".
---

# Skill: create-feature

Produz um documento completo de **definição de feature**. Esta etapa é apenas documentação — sem arquitetura, sem código.

## Inputs
- **FeatureName** e uma breve descrição da necessidade de negócio.

## Steps
1. Adote a mentalidade do agente `product-planner`.
2. Esclareça ambiguidades: liste questões em aberto e proponha padrões; não trave em busca da perfeição.
3. Preencha `templates/feature-template.md`:
   - Declaração do problema, contexto de negócio.
   - Resultados / métricas de sucesso (mensuráveis).
   - Dentro do escopo / fora do escopo.
   - Critérios de aceitação como Given/When/Then.
   - Requisitos não funcionais (latência, throughput, auditabilidade, retenção, segurança).
   - Dependências e riscos.
4. Escreva o resultado em `docs/features/<feature-kebab>.md`.
5. Adicione uma entrada inicial no índice `docs/features/README.md` (crie se não existir).

## Suggested agents
`product-planner`.

## Done when
O doc da feature tem critérios de aceitação concretos e está pronto para refinamento/arquitetura.
Próximo passo a sugerir ao usuário: **"faça um brainstorm da feature <name>"** (refinar) e depois
**"abra arquitetura da feature <name>"**.
