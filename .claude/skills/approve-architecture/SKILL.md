---
name: approve-architecture
description: Projeta e registra a arquitetura técnica de uma feature definida dentro da Clean Architecture — casos de uso, portas, modelo Oracle, mensageria, jobs, resiliência, observabilidade — e captura decisões como ADRs. Use quando o usuário disser "abra arquitetura da feature Z".
---

# Skill: approve-architecture

Transforma uma definição de feature em um design técnico aprovado mais quaisquer ADRs.

## Inputs
- **FeatureName** (já deve existir `docs/features/<feature>.md`). Se estiver faltando, execute `/create-feature` primeiro.

## Steps
1. Adote a mentalidade do agente `solution-architect`. Leia o doc da feature + `docs/standards/**` + ADRs existentes.
2. Identifique os **casos de uso** (commands/queries) com contratos de request/response e regras de validação.
3. Mapeamento de camadas (Domain / Application / Infrastructure / Api) conforme `docs/standards/architecture.md`.
4. **Modelo de dados Oracle**: tabelas, chaves, índices, constraints e o plano do script de migração
   (adie a escrita dos scripts para `/create-oracle-script`).
5. **Mensageria**: topics/filas necessárias, provedor (plugável), contratos de mensagem, estratégia de idempotência.
6. **Jobs**: qualquer trabalho recorrente/fire-and-forget no Hangfire e seu agendamento.
7. **Resiliência**: quais chamadas recebem retry / circuit breaker / timeout do Polly.
8. **Observabilidade**: lista de spans, métricas, eventos de logs estruturados + nomes das propriedades.
9. Registre novas decisões transversais como ADRs via `templates/adr-template.md`.
10. Escreva `docs/architecture/<feature>.md` a partir de `templates/architecture-template.md`.

## Suggested agents
`solution-architect` (líder), consultando `oracle-dba-reviewer`, `observability-engineer`,
`security-reviewer`; revisão final por `tech-lead-reviewer`.

## Done when
O doc de arquitetura lista casos de uso concretos e nomeados, prontos para `/create-usecase`, e todas as novas decisões
estão capturadas como ADRs. Próximo passo a sugerir: **"implemente o use case <name>"**.
