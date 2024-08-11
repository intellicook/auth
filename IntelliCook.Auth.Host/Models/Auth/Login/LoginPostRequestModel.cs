using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Host.Models.Auth.Login;

public class LoginPostRequestModel
{
    [Required] public string Username { get; set; }

    [Required] public string Password { get; set; }
}