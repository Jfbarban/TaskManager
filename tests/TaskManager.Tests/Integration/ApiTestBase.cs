using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Domain.Enums;
using TaskManager.Web.Auth;

namespace TaskManager.Tests.Integration;

public abstract class ApiTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    private readonly ApiFactory _factory;

    /// <summary>Cliente sin autenticar. Llama a <see cref="LoginAsync"/> para añadir el token.</summary>
    protected HttpClient Client { get; }

    protected ApiTestBase()
    {
        _factory = new ApiFactory();
        _factory.InicializarBaseDeDatos();

        // Base https para que UseHttpsRedirection no intervenga.
        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>Hace login real y deja el token en las siguientes peticiones del cliente.</summary>
    protected async Task<string> LoginAsync()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { usuario = ApiFactory.Usuario, password = ApiFactory.Password },
            Json);
        response.EnsureSuccessStatusCode();

        var login = await LeerAsync<LoginResponse>(response);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        return login.Token;
    }

    protected static async Task<T> LeerAsync<T>(HttpResponseMessage response)
    {
        var contenido = await response.Content.ReadFromJsonAsync<T>(Json);
        Assert.NotNull(contenido);
        return contenido!;
    }

    protected static async Task<ValidationProblemDetails> LeerValidacionAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return await LeerAsync<ValidationProblemDetails>(response);
    }

    protected static void AssertCampoConError(ValidationProblemDetails problema, string campo) =>
        Assert.Contains(problema.Errors.Keys, k => string.Equals(k, campo, StringComparison.OrdinalIgnoreCase));

    // ---------- Datos de apoyo creados a través de la propia API ----------

    protected async Task<ResponsableDto> CrearResponsableAsync(string nombre = "Ana")
    {
        var response = await Client.PostAsJsonAsync(
            "/api/responsables", new ResponsableCreateDto { Nombre = nombre }, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await LeerAsync<ResponsableDto>(response);
    }

    protected async Task<TareaDto> CrearTareaAsync(
        int responsableId,
        string titulo = "Tarea de prueba",
        EstadoTarea estado = EstadoTarea.Pendiente,
        DateOnly? fechaLimite = null)
    {
        var dto = new TareaCreateDto
        {
            Titulo = titulo,
            Estado = estado,
            FechaLimite = fechaLimite ?? Hoy.AddDays(7),
            ResponsableId = responsableId
        };

        var response = await Client.PostAsJsonAsync("/api/tareas", dto, Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await LeerAsync<TareaDto>(response);
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}