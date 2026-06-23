# Padrão: Integrações Plugáveis (+ catálogo)

Toda integração externa (e-mail, SMS, storage, pagamentos, …) segue o **mesmo padrão plugável** de filas e
banco: **porta na Application + adapters por provedor na Infrastructure**, selecionados por configuração.
A escolha do provedor é **informada pelo catálogo** em [`docs/integrations/`](../integrations/). Ver
[ADR-0016](../adr/0016-pluggable-integrations-catalog.md).

## Forma
- **Porta** (Application/Ports/<Categoria>): ex.: `IEmailSender`, `ISmsSender`, `IFileStorage`, `IPaymentGateway`.
- **Adapter** (Infrastructure/Integrations/<Categoria>/<Provider>/): implementa a porta com o SDK do provedor.
- **Seleção por config**: ex.: `Integrations:Email:Provider = Smtp | SendGrid | Postmark | Ses | Mailgun`.
- **Segredos** só em variável de ambiente / secret store (nunca no código/JSON versionado).
- **Resiliência + observabilidade**: chamadas externas usam pipeline Polly e emitem spans/logs/métricas.

## O catálogo (docs/integrations)
Cada categoria tem `docs/integrations/<categoria>/README.md` listando os provedores suportados com:
prós/contras, custo, requisitos, config e secrets. É a **fonte de decisão**.

## Como os agentes usam o catálogo (obrigatório)
- O **solution-architect**, ao desenhar uma feature que precise de integração, **lê** o catálogo da
  categoria e **recomenda o provedor** (registrando a escolha na arquitetura/ADR).
- O **backend-developer**, ao implementar, **lê** o doc do provedor escolhido para config/secrets/SDK.
- Skills relacionadas (`/create-integration`, `/create-usecase`, `/approve-architecture`) referenciam o
  catálogo antes de decidir/codar.

## Adicionar uma integração
Use `/create-integration` (escolhe categoria + provedor do catálogo, cria porta se nova categoria e o
adapter, com config e secrets). Categorias atuais: ver [`docs/integrations/README.md`](../integrations/README.md).
