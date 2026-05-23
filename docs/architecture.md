# Architecture

TinyValidations has two main parts:

- runtime library
- source generator

The runtime library contains the public API, execution types, generated-code handshake, and dependency injection registration.

The source generator reads validation declarations and emits validation runners.

## Runtime Project

```text
src/TinyValidations
  Abstractions
  DependencyInjection
  Execution
  Generation
  Rules
```

## Source Generator Project

```text
src/TinyValidations.SourceGen
  Analysis
  Model
  Planning
  Emission
  Generation
```

## Generation Flow

```text
Analysis
  -> Model
  -> Planning
  -> Emission
```

## Analysis

Analysis reads Roslyn syntax and symbols.

It finds classes implementing `IValidation<T>` and reads calls made inside `Define`.

## Model

The model layer contains TinyValidations-owned data structures.

Roslyn types should not leak past analysis.

## Planning

Planning turns analyzed declarations into generated runner plans.

This is where generated names and custom rule registration information are prepared.

## Emission

Emission writes generated C# source.

Generated runners implement `ITinyValidationRunner<T>`.

Generated contributions register runners and custom rules with Microsoft dependency injection.

## Runtime Execution

`ITinyValidator` resolves all generated runners for the command type and asks each runner to validate the command.

Errors are collected into a `ValidationResult`.
