# TinySuite and the sample app

TinyValidations is part of TinySuite, the small family of libraries made of TinyDispatcher, TinyValidations, and TinyEvents.

Each package stays focused:

- TinyValidations owns application input validation.
- TinyDispatcher owns command and query execution.
- TinyEvents owns reliable application-event handling through the outbox pattern.

## TheTinyApplicationLayer sample

The shared sample lives in the sibling `TheTinyApplicationLayer` repository.

It is an ASP.NET Core and Blazor application that uses the three TinySuite NuGet packages together:

```text
Blazor Form
-> API Endpoint
-> TinyValidations
-> TinyDispatcher
-> Use Case
-> TinyEvents Outbox
-> Worker
-> Event Consumer
```

TinyValidations appears before dispatch. It validates the incoming command and lets the handler run only when the command is valid. TinyDispatcher then executes the use case, and TinyEvents records the durable side effect.

Use the sample when you want to see how TinyValidations fits with the rest of TinySuite in a real application shape rather than as an isolated package demo.
