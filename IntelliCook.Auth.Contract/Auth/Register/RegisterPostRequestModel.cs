using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.Auth.Register;

public class RegisterPostRequestModel
{
    [Required]
    [MaxLength(256)]
    [RegularExpression(
        @"^.*\S.*$",
        ErrorMessage = "Name cannot contain only whitespace characters."
    )]
    public string Name { get; set; }

    [Required]
    [MaxLength(256)]
    [RegularExpression(
        @"^[0-9a-zA-Z-\._@\+]+$",
        ErrorMessage = "Username can only contain letters, digits, '-', '.', '_', '@', and '+'."
    )]
    public string Username { get; set; }

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(
        @"^(?=.*[a-z]{1,})(?=.*[A-Z]{1,})(?=.*[0-9]{1,}).+$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit."
    )]
    public string Password { get; set; }
}