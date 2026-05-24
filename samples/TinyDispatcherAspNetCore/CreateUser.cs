using TinyDispatcher;

namespace TinyDispatcherAspNetCore;

public sealed record CreateUser(string Email, string Name, int Age) : ICommand;
