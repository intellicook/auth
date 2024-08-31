using IntelliCook.Auth.Contract.User;

namespace IntelliCook.Auth.Host.Options;

public class AdminOptions : IAuthOptions
{
    public static string SectionKey => "Admin";

    public string Name { get; init; } = null!;
    public UserRoleModel Role { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;

    public void Validate()
    {
        if (Name is null)
        {
            throw new InvalidOperationException("Name is required");
        }

        if (Username is null)
        {
            throw new InvalidOperationException("Username is required");
        }

        if (Email is null)
        {
            throw new InvalidOperationException("Email is required");
        }

        if (Password is null)
        {
            throw new InvalidOperationException("Password is required");
        }
    }
}