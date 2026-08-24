# Roadmap

TinyValidations has a stable 1.0 core package.

This roadmap describes the expected direction, not a compatibility promise.

## Near Term

- Keep adding behavioral tests only where they protect user-visible contracts.
- Keep the stable core small while integrations evolve separately.
- Improve documentation where real usage exposes unclear edges.

## Next Beta Gate

The next TinyValidations beta includes two related contracts:

- the generated validation structure exposes the typed metadata required by provider adapters;
- `Required<TValue>` generates valid behavior for every accepted member type, including `Guid`.

Both contracts are implemented and covered by compilation and runtime behavior tests. Before the
beta is published, validate them together through a clean package consumer using the packed NuGet.

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

## OpenTelemetry

- Completed validation enriches the current activity with aggregate outcome and failure
  count without emitting error detail.
- Standalone validation does not create an activity.
- Keep application operation outcome ownership with the dispatcher, mediator, or host.
