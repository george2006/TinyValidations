namespace Sales;

public sealed class CustomerAccountStore
{
    public bool IsBlocked(string email)
    {
        return email == "blocked@example.com";
    }
}
