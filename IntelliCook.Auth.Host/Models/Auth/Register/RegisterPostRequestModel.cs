namespace IntelliCook.Auth.Host.Models.Auth.Register;

public class RegisterPostRequestModel
{
    public string Name { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
}