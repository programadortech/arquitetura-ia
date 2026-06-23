#!/usr/bin/env bash
# pre-commit-check.sh
# Intended to be wired as a git pre-commit hook (symlink/copy into .git/hooks)
# OR run manually before committing. Blocks the commit (exit 1) on failure.
set -euo pipefail

echo "▶ pre-commit-check: running quality gate…"

fail=0
note() { echo "  ✗ $1"; fail=1; }
ok()   { echo "  ✓ $1"; }

# 1) No secrets staged.
if git diff --cached --name-only | grep -Eiq '(^|/)(\.env|.*\.pfx|.*\.pem|id_rsa|appsettings\.Secrets\.json)$'; then
  note "Potential secret file staged. Remove it before committing."
else
  ok "No obvious secret files staged."
fi

# 2) No paid MediatR usings staged.
if git diff --cached -U0 | grep -Eq '^\+.*using +MediatR'; then
  note "Staged code uses MediatR. Use the in-repo IUseCaseDispatcher abstraction."
else
  ok "No MediatR usage introduced."
fi

# 3) Build (if a solution exists).
if ls ./*.sln >/dev/null 2>&1 || ls ./**/*.sln >/dev/null 2>&1; then
  if command -v dotnet >/dev/null 2>&1; then
    if dotnet build -warnaserror --nologo -v quiet >/dev/null 2>&1; then
      ok "dotnet build succeeded (warnings as errors)."
    else
      note "dotnet build failed. Fix before committing."
    fi
  fi
fi

if [ "$fail" -ne 0 ]; then
  echo "✗ pre-commit-check failed. Commit aborted."
  exit 1
fi
echo "✓ pre-commit-check passed."
exit 0
