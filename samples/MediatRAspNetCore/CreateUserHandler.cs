using MediatR;

namespace MediatRAspNetCore;

public sealed class CreateUserHandler : IRequestHandler<CreateUser, CreateUserResult>
{
    private readonly UserStore _users;

    public CreateUserHandler(UserStore users)
    {
        _users = users;
    }

    public Task<CreateUserResult> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        _users.Add(request.Email);

        return Task.FromResult(new CreateUserResult(request.Email, request.Name, request.Age));
    }
}
