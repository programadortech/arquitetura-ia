#!/usr/bin/env bash
# pre-pr-check.sh
# Run before opening a PR. Aggregates the PowerShell validation scripts + tests.
# Exit 1 on any failure so CI / the developer stops and fixes.
set -euo pipefail

echo "▶ pre-pr-check: full quality gate"

fail=0
run() {
  echo "── $1"
  if eval "$2"; then echo "  ✓ ok"; else echo "  ✗ failed"; fail=1; fi
}

PWSH="pwsh"
command -v pwsh >/dev/null 2>&1 || PWSH="powershell"

run "Clean Architecture rules" "$PWSH -NoProfile -File scripts/validate-clean-architecture.ps1"
run "Architecture docs present" "$PWSH -NoProfile -File scripts/validate-architecture.ps1"
run "DB scripts"               "$PWSH -NoProfile -File scripts/validate-db-scripts.ps1"
run "Tests present & passing"   "$PWSH -NoProfile -File scripts/validate-tests.ps1"
run "PR metadata"              "$PWSH -NoProfile -File scripts/validate-pr.ps1"

if [ "$fail" -ne 0 ]; then
  echo "✗ pre-pr-check failed — do not open the PR until green."
  exit 1
fi
echo "✓ pre-pr-check passed — ready for review."
exit 0
