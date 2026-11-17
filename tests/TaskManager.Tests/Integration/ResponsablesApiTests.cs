using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Responsables;

namespace TaskManager.Tests.Integration;

public class ResponsablesApiTests : ApiTestBase
{
    [Fact]
    public async Task Crear_ConDatosValidos_Devuelve201ConLocation()
    {
        await LoginAsync();

        var response = await Client.PostAsJsonAsync(
            "/api/responsables", new ResponsableCreateDto { Nombre = "Pedro" }, Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var creado = await LeerAsync<ResponsableDto>(response);
        Assert.Equal("Pedro", creado.Nombre);
        Assert.EndsWith($"/api/responsables/{creado.Id}", response.Headers.Location!.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public async Task Crear_ConNombreInvalido_Devuelve400(string nombre)
    {
        await LoginAsync();

        var response = await Client.PostAsJsonAsync("/api/responsables", new { nombre }, Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "Nombre");
    }

    [Fact]
    public async Task Crear_ConJsonMalFormado_Devuelve400()
    {
        await LoginAsync();

        var contenido = new StringContent("{ esto no es json", Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/responsables", contenido);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Listar_DevuelveLosResponsablesOrdenadosPorNombre()
    {
        await LoginAsync();
        await CrearResponsableAsync("Zoe");
        await CrearResponsableAsync("Ana");

        var response = await Client.GetAsync("/api/responsables");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lista = await LeerAsync<List<ResponsableDto>>(response);
        Assert.Equal(new[] { "Ana", "Zoe" }, lista.Select(r => r.Nombre));
    }

    [Fact]
    public async Task Obtener_Existente_Devuelve200()
    {
        await LoginAsync();
        var creado = await CrearResponsableAsync("Marta");

        var response = await Client.GetAsync($"/api/responsables/{creado.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Marta", (await LeerAsync<ResponsableDto>(response)).Nombre);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404ConProblemDetails()
    {
        await LoginAsync();

        var response = await Client.GetAsync("/api/responsables/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problema = await LeerAsync<ProblemDetails>(response);
        Assert.Equal(404, problema.Status);
    }

    [Fact]
    public async Task Eliminar_SinTareas_Devuelve204YYaNoExiste()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync("Temporal");

        var borrado = await Client.DeleteAsync($"/api/responsables/{responsable.Id}");
        var consulta = await Client.GetAsync($"/api/responsables/{responsable.Id}");

        Assert.Equal(HttpStatusCode.NoContent, borrado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_ConTareasAsignadas_Devuelve409YNoLoBorra()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync("Ocupado");
        await CrearTareaAsync(responsable.Id);

        var borrado = await Client.DeleteAsync($"/api/responsables/{responsable.Id}");
        var consulta = await Client.GetAsync($"/api/responsables/{responsable.Id}");

        Assert.Equal(HttpStatusCode.Conflict, borrado.StatusCode);
        var problema = await LeerAsync<ProblemDetails>(borrado);
        Assert.Equal(409, problema.Status);
        Assert.Contains("Ocupado", problema.Detail);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        await LoginAsync();

        var response = await Client.DeleteAsync("/api/responsables/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}