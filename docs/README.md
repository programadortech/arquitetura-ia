# Documentação

O registro de design duradouro do template e dos projetos gerados a partir dele.

| Pasta | Conteúdo |
|---|---|
| [`standards/`](standards/) | **Regras vinculantes** que todo projeto deve seguir (arquitetura, dispatcher, Oracle, observabilidade, resiliência, filas, jobs, testes, checklist de qualidade). Inclui o [padrão de escrita de histórias para POs](standards/escrita-de-historias.md). |
| [`adr/`](adr/) | Architecture Decision Records — o *porquê* por trás dos padrões. |
| [`architecture/`](architecture/) | Designs técnicos por feature (a partir de `templates/architecture-template.md`). |
| [`features/`](features/) | Definições por feature (a partir de `templates/feature-template.md`). |
| [`usecases/`](usecases/) | Catálogo e especificações de casos de uso. |
| [`database/`](database/) | Documentação do modelo de dados Oracle, notas de ER, log de migração. |
| [`observability/`](observability/) | Contratos de telemetria, dashboards, catálogos de log/span/métrica. |
| [`testing/`](testing/) | Planos de teste e mapas de cobertura. |

Comece por [`standards/`](standards/) e pelo [índice de ADR](adr/). Novos trabalhos fluem:
feature → arquitetura → caso de uso → testes → revisão de PR (veja as skills em `.claude/skills/`).
