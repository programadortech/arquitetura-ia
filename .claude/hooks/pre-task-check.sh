#!/usr/bin/env bash
# pre-task-check.sh
# UserPromptSubmit hook. Emits lightweight context so Claude is reminded of the
# standards before acting. Non-blocking (exit 0). stdout is added to context.
set -euo pipefail

cat <<'EOF'
[template-guardrails]
- This is an enterprise TEMPLATE repo. For "crie projeto/feature/usecase/job/fila",
  invoke the matching skill in .claude/skills/ instead of improvising.
- Mandatory stack: C#/.NET 10, Clean Architecture, custom UseCase dispatcher (no paid MediatR),
  Oracle, Serilog + OpenTelemetry, Polly, Hangfire, pluggable queues (Kafka/SQS/RabbitMQ/MQTT).
- New architectural decisions require an ADR (templates/adr-template.md).
- Respect the dependency rule: Domain <- Application <- {Infrastructure, Api}.
EOF

exit 0
