using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserPutResponseModel
{
    [Required] public string AccessToken { get; set; }
}