---
name: brainstorm-story
description: Faz um brainstorm/refinamento colaborativo de uma história ou feature — analisa o que está escrito, expõe ambiguidades, lacunas e casos de borda, faz perguntas direcionadas e reescreve a história mais clara, completa e pronta para virar software. Use quando o usuário disser "faça um brainstorm da história X", "refine a feature Y" ou "melhore a história antes de codar".
---

# Skill: brainstorm-story

Transforma uma história rasa/ambígua em uma **especificação clara e implementável**, sem inventar
requisito: o que não estiver claro vira **pergunta** ou **premissa marcada "a confirmar"**.
Roda **depois** de `/import-story` ou `/create-feature` e **antes** de `/approve-architecture`.

## Entradas
- O documento da história: `docs/features/<KEY>-<slug>.md` (ou um texto cru de história).
- Contexto disponível: `body`, critérios de aceite, `children` (tasks) e `parent` importados do tracker.

## Processo
1. **Ler e resumir** a história em 2–3 linhas (o que se entende hoje) e listar o que está **faltando/ambíguo**.
2. **Analisar por lentes** (marcar cada item como OK / lacuna / pergunta):
   - **Clareza & objetivo:** persona, objetivo e valor estão explícitos? Termos do domínio definidos?
   - **Escopo:** o que entra e o que NÃO entra? Está grande demais (deve ser fatiada)?
   - **Critérios de aceite:** existem, são **testáveis** e cobrem **caminho feliz + validações + erros + bordas**?
   - **Regras de negócio:** estão explícitas e verificáveis? Há cálculos/limites/estados?
   - **Dados:** entidades, volume, retenção, migração, consistência.
   - **NFRs:** performance, confiabilidade/idempotência, segurança/authz/PII, observabilidade.
   - **Integrações/dependências:** sistemas, filas, APIs, jobs, times.
   - **Riscos & premissas:** o que pode dar errado; o que estamos assumindo.
3. **Perguntar ao usuário** as dúvidas que mudam o resultado (use perguntas objetivas, com opções/recomendação
   quando possível). Não prosseguir "no escuro" em pontos que alteram o software; para o resto, propor defaults.
4. **Reescrever a história** melhorada no próprio `docs/features/<KEY>.md`:
   - história/contexto mais nítidos; **regras de negócio** listadas; **critérios de aceite Given/When/Then**
     reforçados (sucesso, validação, erro, borda); NFRs preenchidos; dependências e riscos.
   - sugerir **fatiamento** (INVEST) se a história for grande: propor sub-histórias com escopo próprio.
   - registrar **premissas** e **perguntas em aberto** numa seção, marcando "a confirmar".
   - acrescentar uma seção **"Histórico de refinamento"** (data + principais decisões/respostas).
5. **(Opcional) Write-back ao tracker:** oferecer atualizar a descrição/critérios de aceite no item de origem
   (se o usuário quiser e houver credencial de escrita) — caso contrário, manter só no doc versionado.

## Boas práticas
- Refinar **a história** (não as tasks): a história dá o contexto; as `children` ajudam a entender o recorte.
- Critério bom é **observável e testável** — se não dá para escrever o teste, ainda está ambíguo.
- Preferir perguntas que destravam decisão de software; evitar perguntas cosméticas.
- Não transformar isso em desenho técnico (isso é `/approve-architecture`).

## Agentes sugeridos
`product-planner` (lidera) com lentes de `solution-architect` (viabilidade) e `qa-tester` (testabilidade).

## Concluído quando
A história está clara, com critérios de aceite testáveis, NFRs, premissas e perguntas resolvidas/registradas —
pronta para **`/approve-architecture`**. Próximo passo a sugerir: **"abra arquitetura da feature <KEY>"**.
