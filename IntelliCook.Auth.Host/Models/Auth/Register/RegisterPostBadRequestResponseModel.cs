namespace IntelliCook.Auth.Host.Models.Auth.Register;

public class RegisterPostBadRequestResponseModel
{
    public IList<string> Name { get; set; } = new List<string>();

    public IList<string> Username { get; set; } = new List<string>();

    public IList<string> Email { get; set; } = new List<string>();

    public IList<string> Password { get; set; } = new List<string>();
}