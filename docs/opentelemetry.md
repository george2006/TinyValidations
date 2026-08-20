# OpenTelemetry

TinyValidations enriches the current `System.Diagnostics.Activity` when validation
completes.

```text
tiny.validation.outcome       success | failure
tiny.validation.failure.count number of collected validation errors
```

This works with an activity created by TinyDispatcher, MediatR instrumentation, ASP.NET
Core, or application code. TinyValidations does not require an OpenTelemetry package to
write these standard `Activity` tags.

TinyValidations does not create an activity when validation runs without a current
activity. It also does not emit one activity per validation, validator, or rule.

Only the aggregate outcome and count are recorded. Attempted values, member paths, rule
names, and validation messages are not emitted. If a validation runner throws or is
canceled before producing a result, TinyValidations does not record a completed validation
outcome.

Changing an owning application operation from failure to rejected requires an explicit
contract with that operation's instrumentation. TinyValidations does not overwrite
`tiny.operation.outcome` on its own.
