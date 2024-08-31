using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserPasswordPutRequestModel
{
    [Required] public string OldPassword { get; set; }

    [Required] public string NewPassword { get; set; }
}