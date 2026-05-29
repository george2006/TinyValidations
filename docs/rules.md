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

## Required

Requires the value to be present. For strings, whitespace is treated as missing.

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

Empty values are ignored by `Email`. Use `Required` or `HasText` when the field must be present.

## Text Length

```csharp
rules.TextLengthAtLeast(x => x.Name, 2);
rules.TextLengthAtMost(x => x.Name, 100);
```

Null values are ignored by text length rules. Use `Required`, `HasText`, or `NotNull` when needed.

## Numeric And Comparable Rules

```csharp
rules.Above(x => x.Age, 17);
rules.AtLeast(x => x.Age, 18);
rules.Below(x => x.Age, 130);
rules.AtMost(x => x.Age, 129);
```

## Matches

Validates a non-empty string against a regular expression pattern.

```csharp
rules.Matches(x => x.Code, "^[A-Z]{3}$");
```

Empty values are ignored by `Matches`. Use `Required` or `HasText` when the field must be present.

## Requires

Validates a member with a static requirement method.

```csharp
rules.Requires(x => x.OrderNumber, OrderNumberRequirements.HasOrderPrefix, "Order number must start with ORD-.");
```

Requirement rules are generated as direct static method calls. See [Extending TinyValidations](extending.md).

## Custom Messages

Most rules accept an optional message.

```csharp
rules.Required(x => x.Email, "Email is required.");
rules.AtLeast(x => x.Age, 18, "User must be an adult.");
```

## Custom Rules

Use custom rules for checks that need services, database access, or business logic.

```csharp
rules.Use<UniqueEmailRule>();
```

See [Custom rules](custom-rules.md).
