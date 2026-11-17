using System.ComponentModel.DataAnnotations;

namespace TaskManager.Web.Auth;

public class JwtSettings
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;

    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;
}