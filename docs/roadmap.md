# Roadmap

TinyValidations is beta software.

This roadmap describes the expected direction, not a compatibility promise.

## Near Term

- Add a `.gitignore`.
- Improve source generator diagnostics.
- Add compile-and-execute generator tests.
- Expand sample projects.
- Document all supported syntax shapes.
- Add NuGet package metadata.

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

- Revisit `ValidationErrorCollection` as a public custom-rule surface.
- Decide whether bootstrap registration remains the long-term mechanism.
- Add more built-in helper rules only when they stay small and boring.
