using MediatR;

namespace MediatRAspNetCore;

public sealed record CreateUser(string Email, string Name, int Age) : IRequest<CreateUserResult>;

public sealed record CreateUserResult(string Email, string Name, int Age);
