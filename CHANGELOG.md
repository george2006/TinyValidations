# Changelog

## Unreleased

### Changed

- Reorganized rule analysis to separate argument resolution, rule checks, and constant formatting while preserving supported declarations and generated validation behavior.

### Fixed

- `TV0004` no longer incorrectly states that every unsupported argument must be a constant, including invalid regex patterns and unsupported requirement methods.

## 1.1.0-beta.3 - 2026-10-21

### Added
- Compile-time constant arguments for built-in rule values, regex patterns, and messages, including `const` fields and locals, `nameof`, constant expressions, and casts that remain constant expressions.
- Named and reordered arguments for built-in rules and `Requires`, including omitted optional messages.

### Changed
- Generator diagnostics `TV0001` through `TV0006` are now errors instead of warnings. Invalid declarations fail the build even when `TreatWarningsAsErrors` is disabled.

### Fixed
- One invalid validation declaration no longer prevents source generation for unrelated valid declarations in the same assembly. Invalid declarations are excluded as a whole, and diagnostics are collected across declarations.
- Constant arguments preserve their types and conversions in generated comparisons, including typed nulls, enum conversions, numeric precision, and floating-point signed zero.
- Resolved regex patterns are checked during generation; invalid and null patterns report `TV0004`.
- Generated messages preserve escaped quotes, backslashes, and line breaks. Text length messages display the constant value without generated casts.
- Reordered named arguments no longer exchange regex patterns and messages or misidentify selectors and requirements.

### Notes
- The generator still requires compile-time constants for rule values, patterns, and supplied messages. Use `Requires` or custom async rules for checks that need runtime values.
- Valid declarations still generate source when another declaration is invalid, but reported errors must be resolved before the assembly can build successfully.

## 1.1.0-beta.2 - 2026-08-24

### Added
- Typed validation structure for tooling and provider adapters through `TinyValidationBootstrap.GetValidations()`.
- Validated and declaration `Type` values alongside stable string identities.
- Typed custom-rule metadata for each generated validation declaration.

### Fixed
- `Required<TValue>` now generates compilable behavior for reference, nullable, and non-nullable value types.
- Default non-nullable values such as `Guid.Empty`, numeric zero, `false`, zero-valued enums, and default structs are treated as missing.

### Notes
- Validation structure is collected lazily only when requested.
- Structure inspection does not resolve services, construct validators, or execute validation rules.
- Nullable value types are missing only when `null`; an underlying default value such as `(int?)0` is present.

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
