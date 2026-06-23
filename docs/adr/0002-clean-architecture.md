# ADR-0002: Clean Architecture

- **Status:** Aceito
- **Data:** 2026-06-22

## Contexto
Precisamos de uma estrutura consistente e testável que isole as regras de negócio de frameworks e infraestrutura
para que os projetos permaneçam manuteníveis e as escolhas de infraestrutura continuem substituíveis.

## Decisão
Adotar **Clean Architecture** com quatro camadas: `Domain`, `Application`, `Infrastructure`, `Api`, governadas
pela regra de dependência (apenas para dentro). A Application define **ports**; a Infrastructure as implementa.
Regras completas em [`docs/standards/architecture.md`](../standards/architecture.md), garantidas por NetArchTest
e `scripts/validate-clean-architecture.ps1`.

## Consequências
- (+) A lógica de negócio é agnóstica a frameworks e testável por unidade sem infraestrutura.
- (+) Oracle/filas/telemetria são substituíveis por trás de ports.
- (−) Mais projetos/indireção; pequena cerimônia para funcionalidades triviais (trade-off aceito).

## Alternativas consideradas
- N-tier em camadas: isolamento mais fraco, vaza infraestrutura para o código de negócio.
- Apenas vertical slice: boa ergonomia, mas ainda aplicamos a regra de dependência dentro das slices.
