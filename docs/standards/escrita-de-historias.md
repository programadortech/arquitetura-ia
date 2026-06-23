# Padrão de escrita de histórias (para POs) — Azure DevOps

Este é o formato **obrigatório** para criar User Stories no Azure DevOps. Seguir o padrão garante que a
história seja importável (`/import-story`) e que a IA consiga gerar arquitetura, código e testes
**sem adivinhar**. História fora do padrão volta como "não pronta" (ver *Definition of Ready*).

> Regra de ouro: **os critérios de aceite são o contrato.** Eles viram os testes automatizados.
> Se não dá para escrever o critério, o requisito ainda não está claro.

## Onde preencher (campos do work item)

| Campo do Azure | O que colocar |
|---|---|
| **Title** | Resultado em uma linha, orientado a valor. Curto e específico. |
| **Description** | A história + contexto + regras de negócio + escopo + NFRs (modelo abaixo). |
| **Acceptance Criteria** | Cenários **Given/When/Then** (modelo abaixo). É daqui que saem os testes. |
| **Tags** | Componente/área (ex.: `pagamentos`, `api`, `oracle`) — ajuda a roteirizar. |
| **Iteration** | Sprint/release. |
| **Story Points** | Estimativa. |
| **Priority** | 1–4. |

## Modelo do campo **Description** (copiar e preencher)

```
História
Como <persona>, quero <objetivo>, para <benefício / valor de negócio>.

Contexto / Problema
<Por que isso é necessário agora? Qual dor resolve?>

Regras de negócio
- <regra/validação 1 — clara e verificável>
- <regra/validação 2>

Escopo
Inclui: <o que entra>
Não inclui: <o que fica de fora>

Requisitos não funcionais
- Performance: <meta de tempo de resposta / volume>
- Segurança/Privacidade: <autorização, dados sensíveis/PII, auditoria>
- Confiabilidade: <idempotência, reprocessamento, indisponibilidade de dependência>
- Observabilidade: <o que precisa ser medido/rastreado>

Dependências / Integrações
- <sistema, fila, API, equipe>

Links
- <mockups, documentos, contratos de API>
```

## Modelo do campo **Acceptance Criteria** (Gherkin)

Um cenário por critério. Cubra **caminho feliz + validações + erros/borda**.

```
Cenário: <nome curto do caso de sucesso>
  Dado <contexto/estado inicial>
  Quando <ação do usuário/sistema>
  Então <resultado observável e verificável>

Cenário: <validação que deve falhar>
  Dado <contexto>
  Quando <ação inválida>
  Então <mensagem/erro esperado e estado preservado>

Cenário: <caso de borda / dependência indisponível>
  Dado <contexto>
  Quando <condição excepcional>
  Então <comportamento resiliente esperado>
```

> Pode escrever em português (Dado/Quando/Então) ou inglês (Given/When/Then) — só mantenha o formato.

## Definition of Ready (a história só entra na sprint se…)
- [ ] Title claro e orientado a resultado.
- [ ] Description completa (história + contexto + **regras de negócio** + escopo + NFRs).
- [ ] **Acceptance Criteria em Given/When/Then**, cobrindo sucesso, validação e erro.
- [ ] Dependências e integrações listadas.
- [ ] Estimada (Story Points) e priorizada.

## Definition of Done (a história só fecha quando…)
- [ ] Código implementado seguindo a arquitetura aprovada.
- [ ] Todos os critérios de aceite cobertos por testes automatizados (verdes).
- [ ] Observabilidade e resiliência conforme os NFRs.
- [ ] PR revisado e aprovado; work item referenciado no PR.

## Exemplo preenchido (referência)

**Title:** `Bloquear confirmação de pedido com crédito insuficiente`

**Description**
```
História
Como analista de crédito, quero impedir a confirmação de pedidos cujo cliente não tenha limite
disponível, para evitar inadimplência.

Contexto / Problema
Hoje pedidos são confirmados sem checar o limite, gerando perdas. Precisamos validar no momento da
confirmação.

Regras de negócio
- O limite disponível = limite aprovado − pedidos em aberto.
- Pedido só confirma se valor total <= limite disponível.
- Cliente bloqueado nunca confirma, independente do limite.

Escopo
Inclui: validação na confirmação do pedido.
Não inclui: alteração de limite (outra história).

Requisitos não funcionais
- Performance: confirmação responde em < 500 ms (p95).
- Segurança: ação exige perfil "confirmar pedido"; registrar auditoria de quem confirmou.
- Confiabilidade: consulta de limite com timeout + retry; se o serviço de crédito cair, recusar com erro claro (não confirmar).
- Observabilidade: métrica de pedidos bloqueados por crédito; log estruturado com PedidoId e ClienteId.

Dependências / Integrações
- Serviço de Crédito (API interna).
- Tabela ORACLE PEDIDO.

Links
- Mockup: <url>
```

**Acceptance Criteria**
```
Cenário: Confirmação permitida dentro do limite
  Dado um cliente com limite disponível de 1000
  E um pedido no valor de 800
  Quando o analista confirma o pedido
  Então o pedido fica com status "Confirmado"
  E um evento "PedidoConfirmado" é publicado

Cenário: Confirmação bloqueada por crédito insuficiente
  Dado um cliente com limite disponível de 500
  E um pedido no valor de 800
  Quando o analista confirma o pedido
  Então a confirmação é recusada com a mensagem "Crédito insuficiente"
  E o pedido permanece com status "Pendente"

Cenário: Cliente bloqueado nunca confirma
  Dado um cliente marcado como bloqueado
  Quando o analista confirma qualquer pedido
  Então a confirmação é recusada com a mensagem "Cliente bloqueado"

Cenário: Serviço de crédito indisponível
  Dado que o Serviço de Crédito está fora do ar
  Quando o analista confirma o pedido
  Então a confirmação é recusada com erro "Não foi possível validar o crédito"
  E o pedido permanece com status "Pendente"
```

## Como cada parte vira código (por que o padrão importa)

| Na história | Vira no projeto |
|---|---|
| Title + História | Nome e propósito do caso de uso |
| Regras de negócio | Invariantes no Domain + validações no use case |
| Acceptance Criteria (Given/When/Then) | **Testes** unitários e de integração (1:1) |
| NFR de performance/confiabilidade | Polly (timeout/retry/breaker) + metas de teste |
| NFR de segurança/auditoria | AuthZ no endpoint + logs/auditoria |
| NFR de observabilidade | Spans, métricas e logs estruturados |
| Dependências/Integrações | Ports + adapters (Oracle, filas, APIs) |

## Histórias técnicas (arquitetura / infra / setup)

Nem todo trabalho é feature de negócio. Para **setup do projeto, arquitetura base, pipeline,
observabilidade, segurança transversal**, crie uma **história técnica** (ver
[ADR-0012](../adr/0012-story-types-business-technical.md)):

- **Marque o tipo:** adicione a tag/label `historia-tecnica` (ou `tecnica`/`tech`/`arquitetura`/`infra`)
  no work item — é assim que a importação reconhece o tipo.
- **Critérios de aceite são técnicos e verificáveis**, não Given/When/Then de negócio. Exemplos:
  ```
  - A solução compila com -warnaserror
  - Testes de arquitetura (NetArchTest) verdes (regra de dependência respeitada)
  - Observabilidade base (Serilog + OpenTelemetry via OTLP) configurada
  - Health checks expostos; configuração externalizada (sem segredos no código)
  - ADRs base registrados
  ```
- **Description**: foque em objetivo técnico, motivação (o que destrava para o negócio), escopo e
  NFRs transversais (segurança, observabilidade, resiliência).
- Downstream: a IA encaminha para `/create-project` (setup) e/ou `/approve-architecture` (arquitetura
  base) — **sem** gerar casos de uso de negócio. Template: `templates/historia-tecnica-template.md`.

## Tasks no tracker
Ao planejar a implementação, a IA cria as **tasks** (atividades) como itens-filho da história no
Azure DevOps (ver [ADR-0011](../adr/0011-task-writeback-tracker.md)). Os POs/devs acompanham o progresso
direto no board, com as tasks ligadas à User Story.

## Anti-padrões (evitar)
- Critérios vagos ("funcionar corretamente", "ser rápido") sem valor mensurável.
- História sem Acceptance Criteria, ou critérios que não são testáveis.
- Misturar várias histórias num único work item (quebrar em histórias menores).
- Detalhar **solução técnica** na história (isso é papel da arquitetura, não do PO).
- Colocar dados sensíveis/segredos na descrição.

---
> Dica Azure DevOps: salve este formato como **Work Item Template** do time
> (*Project settings → Team configuration → Templates*) para os POs preencherem com 1 clique.
> Referência de importação: [`issue-trackers.md`](issue-trackers.md).
