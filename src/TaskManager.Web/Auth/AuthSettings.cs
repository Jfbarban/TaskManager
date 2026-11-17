using System.ComponentModel.DataAnnotations;

namespace TaskManager.Web.Auth;

public class AuthSettings
{
    public const string Section = "Auth";

    [Required] public string Usuario { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}