using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Host.Models.Auth.Login;

public class LoginPostResponseModel
{
    [Required] public string AccessToken { get; set; }
}