using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserGetResponseModel
{
    [Required] public string Name { get; set; }

    [Required] public UserRoleModel Role { get; set; }

    [Required] public string Username { get; set; }

    [Required] public string Email { get; set; }
}