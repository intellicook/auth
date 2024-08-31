using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserPutRequestModel
{
    [Required] public string Name { get; set; }

    [Required] public string Username { get; set; }

    [Required] public string Email { get; set; }
}