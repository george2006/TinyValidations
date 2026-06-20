# Roadmap

TinyValidations has a stable 1.0 core package.

This roadmap describes the expected direction, not a compatibility promise.

## Near Term

- Keep adding behavioral tests only where they protect user-visible contracts.
- Keep the stable core small while integrations evolve separately.
- Improve documentation where real usage exposes unclear edges.

## TinyDispatcher

- Add a TinyDispatcher integration package.
- Validate commands before handlers run.
- Define the failure/result contract between validation and dispatch.

## ASP.NET

- Add native ASP.NET integration.
- Support endpoint or request validation patterns.
- Decide how validation errors map to HTTP responses.

## Generator

- Improve symbol-based analysis.
- Report diagnostics for unsupported declarations.
- Reduce silent skips.
- Consider generated source snapshots for tests.

## Runtime

- Keep `ValidationErrorCollection` as the small public custom-rule error contract for 1.0.
- Keep bootstrap registration idempotent per service collection.
- Add more built-in helper rules only when they stay small and boring.
