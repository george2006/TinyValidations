# Architecture

TinyValidations has two main parts:

- runtime library
- source generator

The runtime library contains the public API, execution types, generated-code handshake, and dependency injection registration.

The source generator reads validation declarations and emits validation runners.

## Design Principles

TinyValidations is designed to stay small enough to understand.

- The runtime package is host-agnostic.
- Validation declarations are compile-time input, not runtime rule objects.
- Generated runners use direct C# checks for built-in rules.
- Custom async rules are the extension point for business logic and scoped services.
- Dependency injection registration must be idempotent and predictable.
- Public contracts should stay small and stable.
- Tests should protect user-visible behavior, not private helper trivia.

## Runtime Project

```text
src/TinyValidations
  Abstractions
  DependencyInjection
  Execution
  Generation
  Rules
  Telemetry
```

## Source Generator Project

```text
src/TinyValidations.SourceGen
  Discovery
  Analysis
    Declarations
    RuleInvocations
  Model
  Validation
  Planning
  Emission
  Generation
```

## Generation Flow

```text
Discovery
  -> Analysis
  -> Model
  -> Validation
  -> Planning
  -> Emission
```

## Analysis

Analysis reads Roslyn syntax and symbols.

It finds classes implementing `IValidation<T>` and reads calls made inside `Define`.

Discovery first filters candidate class declarations.

`MemberRuleInvocation` and `RequiresRuleInvocation` resolve the arguments, constant values, and
symbols needed to analyze a single rule call. Both use `RuleArgumentBinder` to find explicit
arguments by parameter, including named and reordered arguments. Omitted optional messages
remain absent so emission can supply the default message.

`MemberRuleAnalyzer` and `RequiresRuleAnalyzer` check the argument list, selector, and rule-specific
constraints before creating a `RuleDefinition`. They return an analysis issue when a check fails.
`Use<T>()` calls are handled separately by `CustomRuleAnalyzer`.

Both member rule analyzers use `ConstantArgumentFormatter` to turn resolved constants into C#
expressions and, where needed, display text. Generated expressions preserve constant types and
conversions. Display text is kept separately so default messages do not contain casts used by
generated checks.

Analysis collects issues per declaration and excludes a declaration with issues as a whole.
It continues examining the remaining declarations.

## Model

The model layer contains TinyValidations-owned data structures.

Symbols and operations stay in analysis. Rule definitions carry resolved type names and source
expressions. Declaration locations and diagnostic descriptors are retained for reporting errors.

## Validation

Validation reports all collected analysis issues and diagnoses empty declarations. Diagnostics
`TV0001` through `TV0006` have error severity.

Only valid, non-empty definitions reach planning. An invalid declaration does not prevent source
generation for unrelated valid declarations, but its errors still fail the consuming build.
If no valid definitions remain, the generator emits no source.

## Planning

Planning turns validated declarations into generated runner plans.

This is where generated names and custom rule registration information are prepared.

## Emission

Emission writes generated C# source.

Generated runners implement `ITinyValidationRunner<T>`.

Generated contributions register runners and custom rules with Microsoft dependency injection.

Generated contributions are added to the runtime bootstrap through module initializers. The bootstrap keeps a process-wide contribution list, but applies contributions only once per `IServiceCollection`.

Generated contributions also expose typed validation structure. `TinyValidationBootstrap.GetValidations()`
collects this structure lazily from loaded contributions so tooling can inspect validated types,
declarations, built-in rules, and custom rule types without executing validation behavior.

## Runtime Execution

`ITinyValidator` resolves all generated runners for the command type and asks each runner to validate the command.

Errors are collected into a `ValidationResult`.
