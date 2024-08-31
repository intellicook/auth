using System.ComponentModel.DataAnnotations;

namespace IntelliCook.Auth.Contract.User;

public class UserPutRequestModel
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
}