namespace TaskManager.Web.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string TipoToken { get; set; } = "Bearer";
    public DateTime ExpiraUtc { get; set; }
}