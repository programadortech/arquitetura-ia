# ADR-P0002 (Plataforma2ASmart.Auth): Administração de usuários, autorização por policy e gate de usuário inativo

- **Status:** Aceita
- **Data:** 2026-06-25
- **Decisores:** Acaciano (tech lead), Claude
- **Escopo:** produto **Plataforma2ASmart.Auth** (decisão específica do produto; ADRs transversais ficam em `docs/adr/` na raiz).
- **Feature:** [AZ-12114 — Cadastro e Edição de Usuário](../features/AZ-12114-cadastro-e-edicao-de-usuario.md)

## Contexto
A AZ-12094 entregou autenticação (Identity + JWT). A AZ-12114 adiciona a **administração de usuários**
(cadastrar/editar, perfis, ativo/inativo) feita por um administrador. Isso levanta três decisões duráveis que
extrapolam um único caso de uso: como **autorizar** as operações administrativas, como representar **ativo/inativo**
e onde aplicar o gate, e como modelar o **e-mail de boas-vindas** cujo envio real é de uma história futura.

## Decisão
1. **Autorização por policy nomeada `Users.Manage`** (não `[Authorize(Roles="...")]` literal no controller). A policy é
   registrada na composição da Api e mapeada para a(s) role(s) administrativa(s); trocar o mapeamento não toca o controller.
2. **Status ativo/inativo como flag `IsActive`** no `ApplicationUser` (default `true`). O **gate de login** (porta
   `IIdentityService` da AZ-12094) passa a recusar usuários inativos — inativar **não** apaga o usuário.
3. **`Name`** como atributo de perfil no `ApplicationUser` (o Identity padrão não possui `Name`).
4. **Roles informadas devem existir**: role inexistente no cadastro/edição → erro de **Validation** (a feature não cria roles).
5. **E-mail de boas-vindas via porta dedicada `IUserWelcomeEmailSender`** (Application), com implementação temporária na
   Infrastructure que apenas **loga** (sem expor a senha) e contém `TODO` para a história futura. Mantém a `IEmailSender`
   genérica (AZ-12094) separada do contrato específico.
6. **Criação de usuário + associação de roles é transacional** (`IUnitOfWork` / transação): falha na associação **não**
   deixa usuário criado parcialmente.

## Consequências
- (+) Autorização desacoplada do nome da role; fácil evoluir para múltiplas roles/policies.
- (+) `IsActive` explícito é legível e auditável; gate central no fluxo de login já existente.
- (+) Porta dedicada de boas-vindas deixa a história futura plugar o envio real sem mexer no caso de uso.
- (−) Alterar `ApplicationUser` exige **migração** (`Name`, `IsActive`) nas tabelas do Identity.
- (−) O gate `IsActive` **altera o fluxo de login da AZ-12094** → exige teste de regressão.

## Alternativas consideradas
- **`[Authorize(Roles="Administrador")]` literal:** mais simples, mas acopla o controller ao nome da role.
- **Lockout nativo do Identity para "inativo":** reaproveita mecanismo, mas mistura semântica de bloqueio temporário
  com inativação administrativa permanente.
- **Reusar `IEmailSender` para boas-vindas:** menos código, mas perde o contrato específico que a história pede para a próxima fase.

## Referências
- [Arquitetura AZ-12114](../architecture/AZ-12114-cadastro-e-edicao-de-usuario.md)
- [ADR-P0001 Identity + JWT](0001-identity-jwt-autenticacao.md)
- Transversais: [ADR-0014 erros](../../../docs/adr/0014-error-handling-result-notification.md) · [ADR-0028 camada de API](../../../docs/adr/0028-padroes-camada-api.md)
