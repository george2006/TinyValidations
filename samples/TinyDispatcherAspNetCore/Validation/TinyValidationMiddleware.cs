using TinyDispatcher;
using TinyDispatcher.Pipeline;
using TinyValidations;
using TinyDispatcherAppContext = TinyDispatcher.AppContext;

namespace TinyDispatcherAspNetCore.Validation;

public sealed class TinyValidationMiddleware<TCommand> : ICommandMiddleware<TCommand, TinyDispatcherAppContext>
    where TCommand : ICommand
{
    private readonly ITinyValidator _validator;

    public TinyValidationMiddleware(ITinyValidator validator)
    {
        _validator = validator;
    }

    public async ValueTask InvokeAsync(
        TCommand command,
        TinyDispatcherAppContext context,
        ICommandPipelineRuntime<TCommand, TinyDispatcherAppContext> runtime,
        CancellationToken cancellationToken = default)
    {
        var result = await _validator.ValidateAsync(command, cancellationToken);

        if (!result.IsValid)
        {
            throw new TinyValidationException(result.Errors);
        }

        await runtime.NextAsync(command, context, cancellationToken);
    }
}
