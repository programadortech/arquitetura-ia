#!/usr/bin/env bash
# dangerous-command-blocker.sh
# PreToolUse hook (matcher: Bash). Reads the tool call from stdin as JSON and
# blocks destructive / unsafe commands. Exit code 2 => block the tool call.
set -euo pipefail

input="$(cat)"

# Extract the command string (best-effort; works with/without jq).
if command -v jq >/dev/null 2>&1; then
  cmd="$(printf '%s' "$input" | jq -r '.tool_input.command // empty')"
else
  cmd="$(printf '%s' "$input" | grep -o '"command"[[:space:]]*:[[:space:]]*"[^"]*"' | sed 's/.*:[[:space:]]*"//; s/"$//')"
fi

block() {
  echo "🛑 BLOCKED by dangerous-command-blocker: $1" >&2
  echo "   Command: ${cmd}" >&2
  exit 2
}

# Destructive filesystem
case "$cmd" in
  *"rm -rf /"*|*"rm -rf ~"*|*"rm -rf ."*) block "recursive force delete of a critical path" ;;
  *":(){:|:&};:"*) block "fork bomb" ;;
esac

# Git history rewriting / force push to shared branches
echo "$cmd" | grep -Eq 'git +push +.*(--force|-f)([^a-zA-Z]|$)' && block "git force push"
echo "$cmd" | grep -Eq 'git +reset +--hard' && block "git reset --hard (data loss)"
echo "$cmd" | grep -Eq 'git +clean +-[a-z]*f' && block "git clean -f (data loss)"

# Secrets / credential exfiltration patterns
echo "$cmd" | grep -Eq '(curl|wget).*(\.env|id_rsa|credentials)' && block "possible secret exfiltration"

# Oracle destructive DDL run ad-hoc from shell
echo "$cmd" | grep -Eiq 'DROP +(TABLE|USER|SCHEMA|DATABASE)' && block "destructive Oracle DDL outside versioned scripts"
echo "$cmd" | grep -Eiq 'TRUNCATE +TABLE' && block "TRUNCATE outside versioned scripts"

exit 0
