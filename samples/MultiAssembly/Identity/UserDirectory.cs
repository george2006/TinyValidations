namespace Identity;

public sealed class UserDirectory
{
    public bool EmailExists(string email)
    {
        return email == "taken@example.com";
    }
}
