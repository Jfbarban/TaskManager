using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskManager.Web.Auth;

namespace TaskManager.Web.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthSettings _auth;
    private readonly ITokenService _tokens;

    public AuthController(IOptions<AuthSettings> auth, ITokenService tokens)
    {
        _auth = auth.Value;
        _tokens = tokens;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login(LoginRequest request)
    {
        // Se evalúan ambos con "&" (no "&&") y comparación en tiempo constante
        var usuarioOk = Iguales(request.Usuario, _auth.Usuario);
        var passwordOk = Iguales(request.Password, _auth.Password);

        if (!(usuarioOk & passwordOk))
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales inválidas",
                detail: "Usuario o contraseña incorrectos.");

        var (token, expira) = _tokens.CrearToken(_auth.Usuario);
        return Ok(new LoginResponse { Token = token, ExpiraUtc = expira });
    }

    private static bool Iguales(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}