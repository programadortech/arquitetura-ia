# ADR-0012: Tipos de história — negócio e técnica

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Nem todo trabalho é uma feature de negócio. Construir um sistema exige histórias de **plataforma/arquitetura**:
setup do projeto, arquitetura base, pipeline de CI, observabilidade transversal, segurança, migrações de
plataforma. Tratar isso como feature de negócio distorce critérios de aceite e o fluxo de implementação.

## Decisão
Reconhecer dois **tipos de história**:
- **Negócio** (`/create-feature`): valor para o usuário; critérios de aceite comportamentais
  (Given/When/Then); fluxo `/approve-architecture` → `/create-usecase`.
- **Técnica** (`/create-tech-story`): plataforma/arquitetura; critérios de aceite **técnicos verificáveis**
  (compila com `-warnaserror`, testes de arquitetura verdes, observabilidade base, ADRs); fluxo
  `/create-project` e/ou `/approve-architecture` **sem** casos de uso de negócio.

Histórias importadas são classificadas pelo label/tag: se contiver algum de
`storyKinds.technicalLabels` (`.claude/tracker.config.json`), são técnicas. Template dedicado em
`templates/historia-tecnica-template.md`.

## Consequências
- (+) Setup e arquitetura base entram no mesmo backlog rastreável, com critérios adequados.
- (+) A IA aplica o fluxo correto por tipo (sem inventar use case de negócio em trabalho de plataforma).
- (+) Decisões transversais das histórias técnicas viram ADRs naturalmente.
- (−) Exige disciplina de marcar o tipo (label/tag) na criação da história.

## Alternativas consideradas
- Só histórias de negócio: força encaixar trabalho de plataforma em um molde errado.
- Usar um tracker/board separado para técnico: fragmenta o backlog e a rastreabilidade.
