# Plano de Testes: <Feature / Use Case>

- **Escopo:** <o que é coberto>
- **Autor:** <nome> · **Data:** <YYYY-MM-DD>
- **Ferramentas:** xUnit · FluentAssertions · NSubstitute · Testcontainers · NetArchTest

## Mapeamento critérios de aceite → testes
| AC | Given/When/Then | Nível | Nome do teste |
|---|---|---|---|
| AC#1 | … | Unit | `Handler_WhenX_ReturnsY` |
| AC#2 | … | Integration | `Repo_RoundTrips_Order` |

## Testes unitários (`<Project>.UnitTests`)
- [ ] Caminho feliz
- [ ] Cada falha de validação
- [ ] Casos de borda: <…>
- [ ] Caminhos de erro/exceção
- Portas mockadas: <lista>. Sem I/O, sem clock/rede reais.

## Testes de integração (`<Project>.IntegrationTests`)
- [ ] Round-trip do repositório Oracle (SQL real)
- [ ] Publish na fila → consume (provedor: <…>)
- [ ] Enqueue/execução do Hangfire (se aplicável)
- Infra: <Testcontainers / schema de teste>. Estado limpo entre os testes.

## Testes de arquitetura (`<Project>.ArchitectureTests`)
- [ ] Regra de dependência
- [ ] Application não referencia Infrastructure
- [ ] Handlers nomeados `*Handler` implementam `IUseCase<,>`
- [ ] Sem referência a `MediatR`

## Verificações não funcionais
- [ ] Asserção(ões) de performance: <…>
- [ ] Idempotência verificada para operações com retry

## Critérios de saída
- [ ] Todos os testes determinísticos e verdes
- [ ] Cada AC mapeado para ≥1 teste
- [ ] `pwsh scripts/validate-tests.ps1` passa
