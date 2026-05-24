using TinyDispatcher;
using TinyDispatcherAppContext = TinyDispatcher.AppContext;

namespace TinyDispatcherAspNetCore;

public sealed class CreateUserHandler : ICommandHandler<CreateUser, TinyDispatcherAppContext>
{
    private readonly UserStore _users;

    public CreateUserHandler(UserStore users)
    {
        _users = users;
    }

    public Task HandleAsync(
        CreateUser command,
        TinyDispatcherAppContext context,
        CancellationToken cancellationToken = default)
    {
        _users.Add(command.Email);

        return Task.CompletedTask;
    }
}
