using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.DTOs.Tareas;

namespace TaskManager.Web.Services;

public interface IApiClient
{
    Task<PagedResult<TareaDto>> ListarTareasAsync(TareaFiltroDto filtro, CancellationToken ct = default);
    Task<TareaDto> ObtenerTareaAsync(int id, CancellationToken ct = default);
    Task<TareaDto> CrearTareaAsync(TareaCreateDto dto, CancellationToken ct = default);
    Task<TareaDto> ActualizarTareaAsync(int id, TareaUpdateDto dto, CancellationToken ct = default);
    Task EliminarTareaAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<ResponsableDto>> ListarResponsablesAsync(CancellationToken ct = default);
    Task<ResponsableDto> ObtenerResponsableAsync(int id, CancellationToken ct = default);
    Task<ResponsableDto> CrearResponsableAsync(ResponsableCreateDto dto, CancellationToken ct = default);
    Task EliminarResponsableAsync(int id, CancellationToken ct = default);
}

public class ApiClient : IApiClient
{
    // Mismo formato que la API: camelCase y estados como texto.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http) => _http = http;

    // ---------- Tareas ----------

    public Task<PagedResult<TareaDto>> ListarTareasAsync(TareaFiltroDto filtro, CancellationToken ct = default)
    {
        var query = new List<string>
        {
            $"pagina={filtro.Pagina}",
            $"tamanoPagina={filtro.TamanoPagina}"
        };
        if (filtro.Estado.HasValue) query.Add($"estado={filtro.Estado}");
        if (filtro.ResponsableId.HasValue) query.Add($"responsableId={filtro.ResponsableId}");

        return EnviarAsync<PagedResult<TareaDto>>(HttpMethod.Get, $"api/tareas?{string.Join('&', query)}", null, ct);
    }

    public Task<TareaDto> ObtenerTareaAsync(int id, CancellationToken ct = default)
        => EnviarAsync<TareaDto>(HttpMethod.Get, $"api/tareas/{id}", null, ct);

    public Task<TareaDto> CrearTareaAsync(TareaCreateDto dto, CancellationToken ct = default)
        => EnviarAsync<TareaDto>(HttpMethod.Post, "api/tareas", dto, ct);

    public Task<TareaDto> ActualizarTareaAsync(int id, TareaUpdateDto dto, CancellationToken ct = default)
        => EnviarAsync<TareaDto>(HttpMethod.Put, $"api/tareas/{id}", dto, ct);

    public Task EliminarTareaAsync(int id, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"api/tareas/{id}", null, ct);

    // ---------- Responsables ----------

    public async Task<IReadOnlyList<ResponsableDto>> ListarResponsablesAsync(CancellationToken ct = default)
        => await EnviarAsync<List<ResponsableDto>>(HttpMethod.Get, "api/responsables", null, ct);

    public Task<ResponsableDto> ObtenerResponsableAsync(int id, CancellationToken ct = default)
        => EnviarAsync<ResponsableDto>(HttpMethod.Get, $"api/responsables/{id}", null, ct);

    public Task<ResponsableDto> CrearResponsableAsync(ResponsableCreateDto dto, CancellationToken ct = default)
        => EnviarAsync<ResponsableDto>(HttpMethod.Post, "api/responsables", dto, ct);

    public Task EliminarResponsableAsync(int id, CancellationToken ct = default)
        => EnviarAsync(HttpMethod.Delete, $"api/responsables/{id}", null, ct);

    // ---------- Infraestructura ----------

    private async Task<T> EnviarAsync<T>(HttpMethod metodo, string url, object? cuerpo, CancellationToken ct)
    {
        using var response = await EnviarCrudoAsync(metodo, url, cuerpo, ct);

        try
        {
            var resultado = await response.Content.ReadFromJsonAsync<T>(Json, ct);
            return resultado ?? throw new ApiException("La API devolvió una respuesta vacía.", response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new ApiException("La respuesta de la API no tiene el formato esperado.", response.StatusCode, ex);
        }
    }

    private async Task EnviarAsync(HttpMethod metodo, string url, object? cuerpo, CancellationToken ct)
    {
        using var response = await EnviarCrudoAsync(metodo, url, cuerpo, ct);
    }

    private async Task<HttpResponseMessage> EnviarCrudoAsync(
        HttpMethod metodo, string url, object? cuerpo, CancellationToken ct)
    {
        using var peticion = new HttpRequestMessage(metodo, url);
        if (cuerpo is not null)
            peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType(), options: Json);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(peticion, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("No se pudo conectar con la API.", null, ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ApiException("La API tardó demasiado en responder.", null, ex);
        }

        if (response.IsSuccessStatusCode) return response;

        var error = await CrearExcepcionAsync(response, ct);
        response.Dispose();
        throw error;
    }

    private static async Task<ApiException> CrearExcepcionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                var validacion = await LeerSeguroAsync<ValidationProblemDetails>(response, ct);
                return new ApiValidationException(
                    validacion?.Detail ?? validacion?.Title ?? "Los datos enviados no son válidos.",
                    validacion?.Errors ?? new Dictionary<string, string[]>());

            case HttpStatusCode.NotFound:
                var noEncontrado = await LeerSeguroAsync<ProblemDetails>(response, ct);
                return new ApiNotFoundException(noEncontrado?.Detail ?? "No se encontró el recurso solicitado.");

            case HttpStatusCode.Conflict:
                var conflicto = await LeerSeguroAsync<ProblemDetails>(response, ct);
                return new ApiConflictException(
                    conflicto?.Detail ?? "La operación entra en conflicto con el estado actual de los datos.");

            case HttpStatusCode.Unauthorized:
                return new ApiException("La web no está autorizada para llamar a la API.", response.StatusCode);

            default:
                return new ApiException("La API respondió con un error inesperado.", response.StatusCode);
        }
    }

    private static async Task<T?> LeerSeguroAsync<T>(HttpResponseMessage response, CancellationToken ct)
        where T : class
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(Json, ct);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return null;
        }
    }
}