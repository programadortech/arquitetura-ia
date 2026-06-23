# ADR-0001: Registrar decisões de arquitetura

- **Status:** Aceito
- **Data:** 2026-06-22
- **Decisores:** Time de arquitetura

## Contexto
Este é um template corporativo que dará origem a muitos projetos. As decisões precisam ser duráveis, descobríveis
e revisáveis com um histórico claro, independente da memória de qualquer indivíduo.

## Decisão
Registramos toda decisão arquitetural significativa como um **ADR** (Architecture Decision Record) usando
`templates/adr-template.md`, numerados sequencialmente (`NNNN-title.md`) em `docs/adr/`. Os ADRs são
imutáveis depois de Aceitos; para alterar uma decisão, adicionamos um novo ADR que **substitui** o antigo.

## Consequências
- (+) Justificativa clara e versionada; onboarding mais rápido; decisões consistentes entre os projetos gerados.
- (+) As revisões podem citar ADRs como fonte da verdade.
- (−) Pequeno custo contínuo de disciplina para escrevê-los.

## Notas
Valores de status: Proposto · Aceito · Descontinuado · Substituído por ADR-XXXX.
