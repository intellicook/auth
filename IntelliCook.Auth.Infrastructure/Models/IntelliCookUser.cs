using IntelliCook.Auth.Contract.User;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Infrastructure.Models;

public class IntelliCookUser : IdentityUser
{
    [MaxLength(256)]
    public string Name { get; set; }

    public UserRoleModel Role { get; set; }

    public override string UserName { get; set; }

    public override string Email { get; set; }

    public override string PasswordHash { get; set; }
}