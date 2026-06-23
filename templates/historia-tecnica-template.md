# História Técnica: <Título>

- **Tipo:** Técnica (arquitetura / infraestrutura / setup) — **não** é feature de negócio
- **Status:** Rascunho | Pronta para arquitetura | Em andamento | Concluída
- **Responsável:** <nome>
- **Data:** <YYYY-MM-DD>
- **Item (tracker):** <KEY / link, ex.: AZ-1234>

> Histórias técnicas constroem ou evoluem o **sistema como um todo** (base do projeto, arquitetura,
> pipeline, observabilidade, segurança transversal). Seguem para `/create-project` e/ou
> `/approve-architecture`, e **não** geram casos de uso de negócio. Decisões transversais viram ADRs.

## Objetivo técnico
<O que esta história habilita/estabelece e por quê. Ex.: "Criar o esqueleto da solução e a arquitetura base".>

## Motivação / Contexto
<Por que agora? Que capacidade ou padrão isto destrava para as histórias de negócio seguintes?>

## Escopo
Inclui:
- <ex.: scaffolding da solução (Domain/Application/Infrastructure/Api + testes)>
- <ex.: dispatcher de use case, Serilog + OpenTelemetry, Polly, Hangfire, provider de fila>
- <ex.: pipeline de CI com os gates de validação>

Não inclui:
- <regras de negócio / features específicas>

## Critérios de aceite técnicos (verificáveis)
- [ ] A solução compila com `-warnaserror`.
- [ ] Os testes de arquitetura (NetArchTest) passam (regra de dependência respeitada).
- [ ] `scripts/validate-clean-architecture.ps1` e `scripts/validate-architecture.ps1` verdes.
- [ ] Observabilidade base configurada (Serilog + OTel exportando via OTLP).
- [ ] Health checks expostos; configuração externalizada (sem segredos no código).
- [ ] ADRs base presentes/atualizados.
- [ ] <outros critérios específicos desta história técnica>

## Requisitos não funcionais transversais
- **Segurança:** <baseline de authz, gestão de segredos>
- **Observabilidade:** <padrão de logs/spans/métricas>
- **Resiliência:** <políticas Polly padrão (oracle/http/queue)>
- **Performance/Custo:** <metas se aplicável>

## Dependências
- <infra (Oracle, broker), credenciais, ambientes>

## Plano de tasks (preenchido por /sync-tasks)
- [ ] <task técnica 1>
- [ ] <task técnica 2>

## ADRs relacionados
- <novos/afetados>

---
> Próximo: `/create-project` (se for o setup inicial) e/ou `/approve-architecture` (arquitetura base).
