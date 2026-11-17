using System.Net;
using System.Net.Http.Json;
using System.Text;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests.Integration;

public class TareasApiTests : ApiTestBase
{
    // ---------- Crear ----------

    [Fact]
    public async Task Crear_ConDatosValidos_Devuelve201ConLocationYCuerpo()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync("Ana");
        var dto = new TareaCreateDto
        {
            Titulo = "Preparar informe",
            Descripcion = "Informe mensual",
            Estado = EstadoTarea.EnCurso,
            FechaLimite = Hoy.AddDays(5),
            ResponsableId = responsable.Id
        };

        var response = await Client.PostAsJsonAsync("/api/tareas", dto, Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var creada = await LeerAsync<TareaDto>(response);
        Assert.Equal("Preparar informe", creada.Titulo);
        Assert.Equal(EstadoTarea.EnCurso, creada.Estado);
        Assert.Equal("Ana", creada.ResponsableNombre);
        Assert.EndsWith($"/api/tareas/{creada.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Crear_SinCamposObligatorios_Devuelve400ConDetallePorCampo()
    {
        await LoginAsync();

        var response = await Client.PostAsJsonAsync("/api/tareas", new { }, Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "Titulo");
        AssertCampoConError(problema, "FechaLimite");
        AssertCampoConError(problema, "ResponsableId");
    }

    [Fact]
    public async Task Crear_ConFechaEnElPasado_Devuelve400()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var dto = new TareaCreateDto
        {
            Titulo = "Tarea vencida",
            FechaLimite = Hoy.AddDays(-1),
            ResponsableId = responsable.Id
        };

        var response = await Client.PostAsJsonAsync("/api/tareas", dto, Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "FechaLimite");
    }

    [Fact]
    public async Task Crear_ConResponsableInexistente_Devuelve400()
    {
        await LoginAsync();
        var dto = new TareaCreateDto
        {
            Titulo = "Sin dueño",
            FechaLimite = Hoy.AddDays(3),
            ResponsableId = 999
        };

        var response = await Client.PostAsJsonAsync("/api/tareas", dto, Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "ResponsableId");
    }

    [Fact]
    public async Task Crear_ConEstadoInvalidoEnElJson_Devuelve400()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var json = $$"""
            { "titulo": "Estado raro", "estado": "Inventado",
              "fechaLimite": "{{Hoy.AddDays(3):yyyy-MM-dd}}", "responsableId": {{responsable.Id}} }
            """;

        var response = await Client.PostAsync(
            "/api/tareas", new StringContent(json, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Obtener ----------

    [Fact]
    public async Task Obtener_Existente_Devuelve200_ConElEstadoComoTexto()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var tarea = await CrearTareaAsync(responsable.Id, estado: EstadoTarea.EnCurso);

        var response = await Client.GetAsync($"/api/tareas/{tarea.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var crudo = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"estado\":\"EnCurso\"", crudo);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        await LoginAsync();

        var response = await Client.GetAsync("/api/tareas/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Actualizar ----------

    [Fact]
    public async Task Actualizar_Existente_Devuelve200ConLosCambios()
    {
        await LoginAsync();
        var ana = await CrearResponsableAsync("Ana");
        var luis = await CrearResponsableAsync("Luis");
        var tarea = await CrearTareaAsync(ana.Id, "Original");

        var dto = new TareaUpdateDto
        {
            Titulo = "Modificada",
            Estado = EstadoTarea.Hecha,
            FechaLimite = Hoy.AddDays(20),
            ResponsableId = luis.Id
        };
        var response = await Client.PutAsJsonAsync($"/api/tareas/{tarea.Id}", dto, Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actualizada = await LeerAsync<TareaDto>(response);
        Assert.Equal("Modificada", actualizada.Titulo);
        Assert.Equal(EstadoTarea.Hecha, actualizada.Estado);
        Assert.Equal("Luis", actualizada.ResponsableNombre);
    }

    [Fact]
    public async Task Actualizar_ConFechaEnElPasado_EsValido()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var tarea = await CrearTareaAsync(responsable.Id);

        var dto = new TareaUpdateDto
        {
            Titulo = "Vencida y terminada",
            Estado = EstadoTarea.Hecha,
            FechaLimite = Hoy.AddDays(-5),
            ResponsableId = responsable.Id
        };
        var response = await Client.PutAsJsonAsync($"/api/tareas/{tarea.Id}", dto, Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Actualizar_ConDatosInvalidos_Devuelve400()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var tarea = await CrearTareaAsync(responsable.Id);

        var response = await Client.PutAsJsonAsync(
            $"/api/tareas/{tarea.Id}",
            new { titulo = "", estado = "Pendiente", fechaLimite = Hoy.AddDays(1), responsableId = responsable.Id },
            Json);

        var problema = await LeerValidacionAsync(response);
        AssertCampoConError(problema, "Titulo");
    }

    [Fact]
    public async Task Actualizar_Inexistente_Devuelve404()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var dto = new TareaUpdateDto
        {
            Titulo = "No existe",
            FechaLimite = Hoy,
            ResponsableId = responsable.Id
        };

        var response = await Client.PutAsJsonAsync("/api/tareas/999", dto, Json);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Eliminar ----------

    [Fact]
    public async Task Eliminar_Existente_Devuelve204YYaNoExiste()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        var tarea = await CrearTareaAsync(responsable.Id);

        var borrado = await Client.DeleteAsync($"/api/tareas/{tarea.Id}");
        var consulta = await Client.GetAsync($"/api/tareas/{tarea.Id}");

        Assert.Equal(HttpStatusCode.NoContent, borrado.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_Inexistente_Devuelve404()
    {
        await LoginAsync();

        var response = await Client.DeleteAsync("/api/tareas/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Listado: filtros y paginación ----------

    [Fact]
    public async Task Listar_FiltraPorEstadoYPorResponsable()
    {
        await LoginAsync();
        var ana = await CrearResponsableAsync("Ana");
        var luis = await CrearResponsableAsync("Luis");
        await CrearTareaAsync(ana.Id, "Ana pendiente", EstadoTarea.Pendiente);
        await CrearTareaAsync(ana.Id, "Ana en curso", EstadoTarea.EnCurso);
        await CrearTareaAsync(luis.Id, "Luis en curso", EstadoTarea.EnCurso);

        var porEstado = await LeerAsync<PagedResult<TareaDto>>(
            await Client.GetAsync("/api/tareas?estado=EnCurso"));
        var porResponsable = await LeerAsync<PagedResult<TareaDto>>(
            await Client.GetAsync($"/api/tareas?responsableId={ana.Id}"));
        var combinado = await LeerAsync<PagedResult<TareaDto>>(
            await Client.GetAsync($"/api/tareas?estado=EnCurso&responsableId={ana.Id}"));

        Assert.Equal(2, porEstado.TotalItems);
        Assert.Equal(2, porResponsable.TotalItems);
        var unica = Assert.Single(combinado.Items);
        Assert.Equal("Ana en curso", unica.Titulo);
    }

    [Fact]
    public async Task Listar_Pagina_DevuelveLosTotalesYLaSegundaPagina()
    {
        await LoginAsync();
        var responsable = await CrearResponsableAsync();
        for (var i = 1; i <= 12; i++)
            await CrearTareaAsync(responsable.Id, $"Tarea {i}", fechaLimite: Hoy.AddDays(i));

        var pagina1 = await LeerAsync<PagedResult<TareaDto>>(await Client.GetAsync("/api/tareas"));
        var pagina2 = await LeerAsync<PagedResult<TareaDto>>(await Client.GetAsync("/api/tareas?pagina=2"));

        Assert.Equal(10, pagina1.Items.Count);      // tamaño por defecto
        Assert.Equal(12, pagina1.TotalItems);
        Assert.Equal(2, pagina1.TotalPaginas);
        Assert.Equal(2, pagina2.Items.Count);
    }

    [Theory]
    [InlineData("pagina=0")]
    [InlineData("tamanoPagina=0")]
    [InlineData("tamanoPagina=51")]
    [InlineData("estado=Inventado")]
    public async Task Listar_ConParametrosInvalidos_Devuelve400(string query)
    {
        await LoginAsync();

        var response = await Client.GetAsync($"/api/tareas?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}