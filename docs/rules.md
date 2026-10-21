# Rules

Rules are declared inside `Define`.

```csharp
public void Define(ValidationRules<CreateUser> rules)
{
    rules.Required(x => x.Email);
}
```

Built-in rules are generated as direct C# checks.

Member selectors can target direct members or nested member paths:

```csharp
rules.Required(x => x.Email);
rules.Required(x => x.Profile.Email);
```

Nested selectors report dotted member paths, such as `Profile.Email`.
Intermediate null values are handled as missing nested values.

## Argument Forms

`Define` is declaration-only; it is not executed at runtime. Declare rule calls directly in this
method. Put conditional logic, calculations, and runtime lookups in a requirement method or a
custom async rule.

Every rule with a member selector accepts a simple member path such as `x => x.Text` or
`x => x.Profile.Name`. Calls such as `x => x.Text.Trim()` are not supported selectors.

| Rules | Other arguments |
| --- | --- |
| `Required`, `HasText`, `NotNull`, `HasItems`, `Email` | Optional compile-time constant string message. |
| `TextLengthAtLeast`, `TextLengthAtMost` | Compile-time constant length accepted by the `int` parameter; optional constant string message. |
| `Above`, `AtLeast`, `Below`, `AtMost` | Compile-time constant value accepted by the inferred or explicit comparable type; optional constant string message. |
| `Matches` | Non-null constant string containing a valid regex pattern; optional constant string message. |
| `Requires` | Static, non-generic method group returning `bool` with one compatible parameter; required constant string message. |
| `Use<TRule>` | A type implementing `IAsyncValidationRule<T>`; no value arguments. |

Supported constant forms include literals, `const` fields and local constants, `nameof`,
parenthesized constants, and expressions or casts that C# can evaluate at compile time:

```csharp
public static class Feedback
{
    public const int MaxTextLength = 4000;
}

public sealed record GiveFeedback(string Text);

public sealed class GiveFeedbackValidation : IValidation<GiveFeedback>
{
    private const string TooLong = "Feedback is too long.";

    public void Define(ValidationRules<GiveFeedback> rules)
    {
        const int minimumLength = 1 + 1;
        rules.TextLengthAtLeast(x => x.Text, minimumLength);
        rules.TextLengthAtMost(x => x.Text, Feedback.MaxTextLength, TooLong);
        rules.HasText(x => x.Text, nameof(GiveFeedback.Text) + " is required.");
    }
}
```

`static readonly` fields, ordinary local variables, property reads, and runtime method calls are
not compile-time constants, even when they always return the same value. These forms produce
`TV0004` when used as a rule value, pattern, or message.

Positional, named, and mixed arguments follow C# binding rules. Reordering named arguments does
not change their meaning:

```csharp
rules.Matches(
    x => x.Code,
    message: "Code must contain three uppercase letters.",
    pattern: "^[A-Z]{3}$");
```

## Declaration Diagnostics

| Diagnostic | Meaning |
| --- | --- |
| `TV0001` | The validation declaration has no usable `Define` method. |
| `TV0002` | The rule call is not supported. |
| `TV0003` | The selector is not a supported member path. |
| `TV0004` | A rule argument or argument list is unsupported, a regex pattern is invalid or null, or a requirement method is unsupported. |
| `TV0005` | The custom rule type does not implement the required `IAsyncValidationRule<T>`. |
| `TV0006` | The validation declaration contains no rules. |

All six diagnostics are errors. They fail the build without requiring `TreatWarningsAsErrors`.
The generator collects diagnostics across declarations instead of stopping after the first
invalid declaration.

A declaration with an analysis issue is excluded as a whole, including any otherwise valid
rules in that declaration. Other valid declarations still generate their runners and
registrations. This isolation does not make the build succeed: the reported errors must be fixed.

## Required

Requires the value to be present. For strings, whitespace is treated as missing.
For non-nullable value types, the default value is treated as missing. Nullable value types are
missing only when they are `null`; an underlying default value such as `(int?)0` is present.

```csharp
rules.Required(x => x.Email);
```

## HasText

Requires a string to contain non-whitespace text.

```csharp
rules.HasText(x => x.Name);
```

## NotNull

Requires a value to be non-null.

```csharp
rules.NotNull(x => x.Profile);
```

## HasItems

Requires an enumerable value to be non-null and contain at least one item.

```csharp
rules.HasItems(x => x.Roles);
```

## Email

Validates a non-empty string as an email address.

```csharp
rules.Email(x => x.Email);
```

Null, empty, and whitespace-only values are ignored by `Email`. Use `Required` or `HasText` when the field must be present.

## Text Length

```csharp
rules.TextLengthAtLeast(x => x.Name, 2);
rules.TextLengthAtMost(x => x.Name, 100);
```

Null values are ignored by text length rules. Use `Required`, `HasText`, or `NotNull` when needed.

## Comparable Rules

```csharp
rules.Above(x => x.Age, 17);
rules.AtLeast(x => x.Age, 18);
rules.Below(x => x.Age, 130);
rules.AtMost(x => x.Age, 129);
```

Comparison rules require a selected type that satisfies `IComparable<T>`. Generated code uses
`Comparer<T>.Default`. The comparison argument must also be a compile-time constant with a
supported C# conversion to that type. Its original type is preserved so user-defined conversions
receive the same typed value as in the declaration.

Numeric, boolean, character, and string constants are supported. Enum constants can be converted
to a custom comparable type when C# permits that conversion; enums themselves do not satisfy the
`IComparable<T>` constraint. Nullable reference comparison values can be constant `null`.

Being comparable does not make a value a compile-time constant. For example, `new DateTime(...)`,
`DateTime.Today`, `default(DateTime)`, and constructed `TimeSpan` or `Guid` values are not supported
comparison arguments. Use `Requires` or an async custom rule for those checks.

For strings, comparison is ordering comparison, not length validation. Use `TextLengthAtLeast`, `TextLengthAtMost`, `HasText`, or `Matches` for text-specific validation.

Nullable reference values are compared with the default comparer. For example, `AtLeast(x => x.Name, "M")` treats `null` as below `"M"`. Use `Required`, `HasText`, or `NotNull` when the value must be present.

## Matches

Validates a non-empty string against a regular expression pattern.

```csharp
rules.Matches(x => x.Code, "^[A-Z]{3}$");
```

The pattern may be a literal or another constant string, such as a `const` field. Invalid and null
patterns produce `TV0004` during generation.

Null, empty, and whitespace-only member values are ignored by `Matches`. Use `Required` or `HasText` when the field must be present.

## Requires

Validates a member with a static requirement method.

```csharp
rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.");
```

Requirement rules are generated as direct static method calls. See [Extending TinyValidations](extending.md).

The requirement method body can contain runtime logic. The constant-argument restriction applies
to the message, not to the code inside that method.

## Custom Messages

Most rules accept an optional compile-time constant string message. `Requires` requires a message.

```csharp
rules.Required(x => x.Email, "Email is required.");
rules.AtLeast(x => x.Age, 18, "User must be an adult.");
```

Omit an optional message to use the default text. Supplied messages must contain non-whitespace
text. An explicit `null`, empty, or whitespace-only message is not a request for the default:
it throws when a failing rule tries to create a validation error.

Constant messages preserve their text, including quotes, backslashes, and line breaks.

## Custom Rules

Use custom rules for checks that need services, database access, or business logic.

```csharp
rules.Use<UniqueEmailRule>();
```

See [Custom rules](custom-rules.md).
