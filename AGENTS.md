# Codex Instructions

This repository is primarily a .NET Blazor project. The `package.json` in the repository exists only for Playwright-driven browser tests. Those tests are currently not used, so the Node tooling is effectively dormant. Unless Playwright testing becomes relevant, you can ignore `npm` and `package.json`. Any Node based test scripts or packages can also be ignored.

## EF Core Migrations

Codex should **not** modify any Entity Framework migration files. Those files live in `BlazorIW/Migrations`. If migrations appear out of date or missing, do not attempt to regenerate them. They will be handled manually outside of Codex.
