using MediatR;
using TinyValidations;

namespace MediatRAspNetCore.Validation;

public sealed class TinyValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITinyValidator _validator;

    public TinyValidationBehavior(ITinyValidator validator)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request, cancellationToken);

        if (!result.IsValid)
        {
            throw new TinyValidationException(result.Errors);
        }

        return await next();
    }
}
