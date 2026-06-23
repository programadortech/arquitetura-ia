# ADR-0017: API Gateway opcional (YARP)

- **Status:** Aceito
- **Data:** 2026-06-23

## Contexto
Quando há mais de uma API, o front precisa de **um único ponto de entrada** (uma base de URL), além de
um lugar para concentrar cross-cutting de borda (roteamento, CORS, rate limit de borda, auth de entrada).

## Decisão
Oferecer, **opcionalmente** no `/create-project` (`gateway: yarp | none`, default `none`), um projeto
`<Produto>.Gateway` baseado em **YARP** (reverse proxy da Microsoft) que roteia para as APIs internas e
expõe uma URL única ao front. Configuração de rotas/clusters via `appsettings`. Regras em
[`docs/standards/api-gateway.md`](../standards/api-gateway.md).

## Consequências
- (+) Front consome um só host; roteamento e políticas de borda centralizados.
- (+) Opcional — projetos single-API não pagam o custo.
- (−) Mais um deployable; cuidar para o gateway não virar lógica de negócio (só roteamento/borda).

## Alternativas consideradas
- Sem gateway (front conhece N hosts): acoplamento e CORS/políticas espalhados.
- Gateway gerenciado (APIM/NGINX/Traefik): válido em produção; YARP é leve, em .NET e versionável no repo.
