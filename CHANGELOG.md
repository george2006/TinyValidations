# Changelog

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

