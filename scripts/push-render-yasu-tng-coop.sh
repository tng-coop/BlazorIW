#!/usr/bin/env bash
set -e
scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$scriptdir/.."

# Push main branch to render-yasu-tng-coop remote branch

git push origin main:render-yasu-tng-coop "$@"
