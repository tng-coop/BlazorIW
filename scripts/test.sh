#!/usr/bin/env bash
set -e
scriptdir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$scriptdir/../BlazorIW.Tests"
dotnet test "$@"
