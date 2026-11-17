using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Web.Auth;

namespace TaskManager.Tests.Integration;

public class AuthApiTests : ApiTestBase
{
    [Fact]
    public async Task Login_ConCredencialesCorrectas_Devuelve200ConToken()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { usuario = ApiFactory.Usuario, password = ApiFactory.Password },
            Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await LeerAsync<LoginResponse>(response);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Assert.Equal("Bearer", login.TipoToken);
        Assert.True(login.ExpiraUtc > DateTime.UtcNow);
    }

    [Theory]
    [InlineData(ApiFactory.Usuario, "contraseña-incorrecta")]
    [InlineData("otro-usuario", ApiFactory.Password)]
    public async Task Login_ConCredencialesIncorrectas_Devuelve401(string usuario, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { usuario, password }, Json);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConCuerpoVacio_Devuelve400ConDetallePorCampo()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { }, Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "Usuario");
        AssertCampoConError(problema, "Password");
    }

    [Theory]
    [InlineData("/api/tareas")]
    [InlineData("/api/tareas/1")]
    [InlineData("/api/responsables")]
    [InlineData("/api/responsables/1")]
    public async Task EndpointsProtegidos_SinToken_Devuelven401(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenValido_Devuelve200()
    {
        await LoginAsync();

        var response = await Client.GetAsync("/api/tareas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenManipulado_Devuelve401()
    {
        var token = await LoginAsync();

        // Cambiamos el primer carácter del payload: la firma deja de coincidir.
        var partes = token.Split('.');
        partes[1] = (partes[1][0] == 'A' ? 'B' : 'A') + partes[1][1..];
        var manipulado = string.Join('.', partes);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", manipulado);
        var response = await Client.GetAsync("/api/tareas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenInventado_Devuelve401()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "esto.no.esunjwt");

        var response = await Client.GetAsync("/api/responsables");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}