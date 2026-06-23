---
name: create-integration
description: Adiciona ou troca uma integração externa plugável (e-mail, SMS, storage, pagamentos, …) escolhendo o provedor a partir do catálogo docs/integrations — cria a porta (se nova categoria) e o adapter na Infrastructure, com config e secrets. Use quando o usuário disser "adicione uma integração de e-mail/SMS/…" ou "troque o provedor de X".
---

# Skill: create-integration

Implementa uma integração externa seguindo o padrão plugável
([`docs/standards/integrations.md`](../../docs/standards/integrations.md)) e **decidindo o provedor pelo
catálogo** ([`docs/integrations/`](../../docs/integrations/README.md)). Ver ADR-0016.

## Entradas
- **Categoria** (ex.: `email`, `sms`, `storage`, `payments`).
- **Provedor** (ex.: `smtp`, `sendgrid`, `postmark`, `ses`, `mailgun`). Se não informado, **recomende** a
  partir do catálogo conforme o contexto do produto e confirme.

## Passos
1. **Ler o catálogo** da categoria: `docs/integrations/<categoria>/README.md` (porta, provedores, prós/contras,
   config, secrets, recomendação). Se a categoria não existir no catálogo, criar o README dela primeiro.
2. **Porta (Application):** se for nova categoria, criar a porta em
   `src/<Produto>.Application/Ports/<Categoria>/I<Categoria>...cs` (ex.: `IEmailSender`) retornando `Result`/`Result<T>`
   (ver `error-handling.md`). Se já existir, reutilizar.
3. **Adapter (Infrastructure):** criar `src/<Produto>.Infrastructure/Integrations/<Categoria>/<Provider>/<Provider>...cs`
   implementando a porta com o SDK do provedor; envolver chamadas em Polly e emitir spans/logs/métricas.
4. **Seleção por config:** `Integrations:<Categoria>:Provider = <Provider>`; registrar o adapter no DI conforme
   a seleção (só o escolhido é registrado). **Secrets só em variável de ambiente/secret store.**
5. **Pacotes:** adicionar o SDK do provedor (central em `Directory.Packages.props`).
6. **Testes:** unit do mapeamento; integração quando fizer sentido (sandbox do provedor / fake).
7. Atualizar `docs/PRODUCT.md` → "Integrações ativas" e, se a decisão for transversal, registrar um ADR.

## Agentes sugeridos
`solution-architect` (escolha do provedor) → `backend-developer` (adapter) → `security-reviewer` (secrets/PII).

## Concluído quando
A integração está atrás da porta, selecionável por config, com secrets fora do código, testada, e o
catálogo/PRODUCT.md refletem o provedor ativo.
