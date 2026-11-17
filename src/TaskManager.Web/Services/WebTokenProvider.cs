using Microsoft.Extensions.Options;
using TaskManager.Web.Auth;

namespace TaskManager.Web.Services;

public interface IWebTokenProvider
{
    string ObtenerToken();
}

/// <summary>
/// Token de servicio con el que la web llama a la API. Se reutiliza hasta
/// un minuto antes de caducar.
/// </summary>
public class WebTokenProvider : IWebTokenProvider
{
    private readonly ITokenService _tokens;
    private readonly AuthSettings _auth;
    private readonly object _lock = new();

    private string? _token;
    private DateTime _expiraUtc;

    public WebTokenProvider(ITokenService tokens, IOptions<AuthSettings> auth)
    {
        _tokens = tokens;
        _auth = auth.Value;
    }

    public string ObtenerToken()
    {
        lock (_lock)
        {
            if (_token is null || DateTime.UtcNow >= _expiraUtc.AddMinutes(-1))
                (_token, _expiraUtc) = _tokens.CrearToken(_auth.Usuario);

            return _token;
        }
    }
}