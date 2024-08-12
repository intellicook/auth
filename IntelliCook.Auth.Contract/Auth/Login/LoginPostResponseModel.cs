using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.Auth.Login;

public class LoginPostResponseModel
{
    [Required] public string AccessToken { get; set; }
}