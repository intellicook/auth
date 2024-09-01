using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserPasswordPutRequestModel
{
    [Required] public string OldPassword { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(
        @"^(?=.*[a-z]{1,})(?=.*[A-Z]{1,})(?=.*[0-9]{1,}).+$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit."
    )]
    public string NewPassword { get; set; }
}