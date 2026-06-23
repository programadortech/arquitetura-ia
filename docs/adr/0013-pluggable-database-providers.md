# ADR-0013: Bancos de dados relacionais plugáveis (Oracle / SQL Server / PostgreSQL / MySQL)

- **Status:** Aceito
- **Data:** 2026-06-23
- **Substitui:** [ADR-0004](0004-oracle-database.md)

## Contexto
O banco fixo (Oracle, [ADR-0004](0004-oracle-database.md)) não atende a todos os times/clientes: alguns
projetos rodam em SQL Server, PostgreSQL ou MySQL. O acesso a dados já está isolado atrás de **ports**
na Application (Clean Architecture), então o motor de banco é um detalhe de Infrastructure — deve ser
**selecionável por configuração**, sem afetar Domain/Application nem o fluxo.

## Decisão
Tornar o banco relacional **plugável**, análogo aos providers de fila ([ADR-0008](0008-pluggable-queue-providers.md)):
- Providers suportados: **Oracle, SQL Server, PostgreSQL, MySQL**. Padrão recomendado: **Oracle**.
- Seleção por configuração `Database:Provider` (no projeto gerado) e parâmetro de `/create-project`.
- Acesso sempre por repositórios que implementam ports da Application; **sempre parametrizado**.
- EF Core com o provider correspondente; apenas o pacote do provider escolhido é referenciado.
- Migrações como scripts **versionados, ordenados e reversíveis** por provider em `db/<provider>/migrations`.
- Convenções e regras em [`docs/standards/database.md`](../standards/database.md); notas por provider
  (ex.: Oracle em [`oracle.md`](../standards/oracle.md)).

## Consequências
- (+) Mesma arquitetura/fluxo independente do banco; troca de motor sem tocar em Domain/Application.
- (+) Portabilidade e adequação por cliente/ambiente.
- (−) SQL de migração é específico por dialeto (uma pasta por provider); cuidado com recursos não portáveis.
- (−) Testes de integração precisam do banco escolhido (Testcontainers tem imagem para os quatro).

## Alternativas consideradas
- Manter Oracle fixo (ADR-0004): inviabiliza times em outros bancos.
- Abstrair só por EF Core sem scripts versionados: perde o controle/rollback explícito em produção.
- ORM agnóstico que esconde o dialeto totalmente: vaza em recursos avançados e tuning; preferimos
  ports + scripts por provider.
