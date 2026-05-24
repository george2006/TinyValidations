namespace TinyDispatcherAspNetCore;

public sealed class UserStore
{
    private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase)
    {
        "existing@example.com"
    };

    public bool Exists(string email) => _emails.Contains(email);

    public void Add(string email) => _emails.Add(email);
}
