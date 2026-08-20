# Changelog

## 1.1.0-beta.1 - 2026-08-20

### Added
- Aggregate validation outcome and failure count on the current `Activity`.
- Behavioral coverage for successful, failed, standalone, and exceptional validation.
- OpenTelemetry usage and privacy documentation.

### Notes
- TinyValidations does not create activities or require an OpenTelemetry package.
- Validation member paths, messages, rule names, and attempted values are not emitted.
- The owning host or dispatcher remains responsible for its operation outcome.

## 1.0.0 - 2026-06-20

### Added
- First stable release of the core compile-time validation package.
- Source-generator diagnostics for invalid validation declarations before generation.
- Compile-backed generated source tests for validation runners.
- Runtime and generator coverage for every built-in rule.
- Null-safe nested member paths with dotted member names in validation errors.
- Comparable rules based on `IComparable<T>` and `Comparer<T>.Default`.
- Regex pattern validation during generation.
- Duplicate custom rule declaration deduplication.
- Multi-assembly validation contribution support with tests and samples.

### Notes
- The core package remains host-agnostic.
- TinyDispatcher, MediatR, and ASP.NET integration shapes are demonstrated through samples.
- Native host integration packages may ship separately so the core stays small.
