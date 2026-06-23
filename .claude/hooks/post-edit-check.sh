#!/usr/bin/env bash
# post-edit-check.sh
# PostToolUse hook (Edit|Write|MultiEdit). Fast, advisory checks on the edited file.
# Non-blocking by default (exit 0) so it never interrupts flow; prints warnings.
set -euo pipefail

input="$(cat)"

if command -v jq >/dev/null 2>&1; then
  file="$(printf '%s' "$input" | jq -r '.tool_input.file_path // empty')"
else
  file="$(printf '%s' "$input" | grep -o '"file_path"[[:space:]]*:[[:space:]]*"[^"]*"' | sed 's/.*:[[:space:]]*"//; s/"$//')"
fi

[ -z "${file:-}" ] && exit 0
[ ! -f "$file" ] && exit 0

warn() { echo "⚠️  post-edit-check ($file): $1" >&2; }

case "$file" in
  *.cs)
    # Layering smell: Application must not reference Infrastructure.
    if printf '%s' "$file" | grep -q '\.Application'; then
      grep -nE 'using +[A-Za-z0-9_.]*\.Infrastructure' "$file" && \
        warn "Application layer references Infrastructure — violates the dependency rule."
    fi
    # Structured logging: discourage string interpolation inside log calls.
    grep -nE 'Log(ger)?\.(LogInformation|LogError|LogWarning|LogDebug)\("?\$' "$file" && \
      warn "Use structured logging (message templates + properties), not interpolated strings."
    # Paid MediatR.
    grep -nE 'using +MediatR' "$file" && \
      warn "MediatR detected — use the in-repo IUseCaseDispatcher abstraction instead."
    ;;
  *.sql)
    grep -niE '\b(DROP|TRUNCATE) ' "$file" >/dev/null && \
      warn "Destructive DDL present — ensure it is guarded and reversible (down script)."
    ;;
esac

exit 0
