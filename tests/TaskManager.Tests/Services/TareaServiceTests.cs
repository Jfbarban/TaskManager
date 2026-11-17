using TaskManager.Application.DTOs.Tareas;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Domain.Enums;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Services;

public class TareaServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TareaService _service;

    public TareaServiceTests() => _service = new TareaService(_db.Context);

    public void Dispose() => _db.Dispose();

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    // ---------- Crear ----------

    [Fact]
    public async Task Crear_ConDatosValidos_DevuelveTareaConResponsable()
    {
        var responsable = await _db.AddResponsableAsync("Ana");
        var dto = new TareaCreateDto
        {
            Titulo = "  Preparar informe  ",
            Descripcion = "Informe mensual",
            Estado = EstadoTarea.EnCurso,
            FechaLimite = Hoy.AddDays(5),
            ResponsableId = responsable.Id
        };

        var resultado = await _service.CrearAsync(dto);

        Assert.True(resultado.Id > 0);
        Assert.Equal("Preparar informe", resultado.Titulo);   // se recortan espacios
        Assert.Equal(EstadoTarea.EnCurso, resultado.Estado);
        Assert.Equal("Ana", resultado.ResponsableNombre);
    }

    [Fact]
    public async Task Crear_ConResponsableInexistente_LanzaBusinessValidationException()
    {
        var dto = new TareaCreateDto
        {
            Titulo = "Tarea huérfana",
            FechaLimite = Hoy.AddDays(1),
            ResponsableId = 999
        };

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() => _service.CrearAsync(dto));

        Assert.Contains(nameof(TareaCreateDto.ResponsableId), ex.Errors.Keys);
    }

    // ---------- Obtener ----------

    [Fact]
    public async Task Obtener_Existente_DevuelveLaTarea()
    {
        var responsable = await _db.AddResponsableAsync();
        var tarea = await _db.AddTareaAsync(responsable.Id, "Mi tarea");

        var resultado = await _service.ObtenerAsync(tarea.Id);

        Assert.Equal("Mi tarea", resultado.Titulo);
    }

    [Fact]
    public async Task Obtener_Inexistente_LanzaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerAsync(999));
    }

    // ---------- Actualizar ----------

    [Fact]
    public async Task Actualizar_Existente_CambiaLosDatos()
    {
        var ana = await _db.AddResponsableAsync("Ana");
        var luis = await _db.AddResponsableAsync("Luis");
        var tarea = await _db.AddTareaAsync(ana.Id, "Original");

        var dto = new TareaUpdateDto
        {
            Titulo = "Modificada",
            Estado = EstadoTarea.Hecha,
            FechaLimite = Hoy.AddDays(10),
            ResponsableId = luis.Id
        };

        var resultado = await _service.ActualizarAsync(tarea.Id, dto);

        Assert.Equal("Modificada", resultado.Titulo);
        Assert.Equal(EstadoTarea.Hecha, resultado.Estado);
        Assert.Equal("Luis", resultado.ResponsableNombre);
    }

    [Fact]
    public async Task Actualizar_Inexistente_LanzaNotFoundException()
    {
        var responsable = await _db.AddResponsableAsync();
        var dto = new TareaUpdateDto
        {
            Titulo = "No importa",
            FechaLimite = Hoy,
            ResponsableId = responsable.Id
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ActualizarAsync(999, dto));
    }

    [Fact]
    public async Task Actualizar_ConResponsableInexistente_LanzaBusinessValidationException()
    {
        var responsable = await _db.AddResponsableAsync();
        var tarea = await _db.AddTareaAsync(responsable.Id);
        var dto = new TareaUpdateDto
        {
            Titulo = "Cambio",
            FechaLimite = Hoy,
            ResponsableId = 999
        };

        await Assert.ThrowsAsync<BusinessValidationException>(() => _service.ActualizarAsync(tarea.Id, dto));
    }

    // ---------- Eliminar ----------

    [Fact]
    public async Task Eliminar_Existente_LaQuitaDeLaBaseDeDatos()
    {
        var responsable = await _db.AddResponsableAsync();
        var tarea = await _db.AddTareaAsync(responsable.Id);

        await _service.EliminarAsync(tarea.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerAsync(tarea.Id));
    }

    [Fact]
    public async Task Eliminar_Inexistente_LanzaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.EliminarAsync(999));
    }

    // ---------- Listar: filtros, orden y paginación ----------

    [Fact]
    public async Task Listar_FiltraPorEstado()
    {
        var responsable = await _db.AddResponsableAsync();
        await _db.AddTareaAsync(responsable.Id, "A", EstadoTarea.Pendiente);
        await _db.AddTareaAsync(responsable.Id, "B", EstadoTarea.EnCurso);
        await _db.AddTareaAsync(responsable.Id, "C", EstadoTarea.EnCurso);

        var resultado = await _service.ListarAsync(new TareaFiltroDto { Estado = EstadoTarea.EnCurso });

        Assert.Equal(2, resultado.TotalItems);
        Assert.All(resultado.Items, t => Assert.Equal(EstadoTarea.EnCurso, t.Estado));
    }

    [Fact]
    public async Task Listar_FiltraPorResponsable()
    {
        var ana = await _db.AddResponsableAsync("Ana");
        var luis = await _db.AddResponsableAsync("Luis");
        await _db.AddTareaAsync(ana.Id, "De Ana 1");
        await _db.AddTareaAsync(ana.Id, "De Ana 2");
        await _db.AddTareaAsync(luis.Id, "De Luis");

        var resultado = await _service.ListarAsync(new TareaFiltroDto { ResponsableId = ana.Id });

        Assert.Equal(2, resultado.TotalItems);
        Assert.All(resultado.Items, t => Assert.Equal(ana.Id, t.ResponsableId));
    }

    [Fact]
    public async Task Listar_CombinaFiltroDeEstadoYResponsable()
    {
        var ana = await _db.AddResponsableAsync("Ana");
        var luis = await _db.AddResponsableAsync("Luis");
        await _db.AddTareaAsync(ana.Id, "Ana pendiente", EstadoTarea.Pendiente);
        await _db.AddTareaAsync(ana.Id, "Ana hecha", EstadoTarea.Hecha);
        await _db.AddTareaAsync(luis.Id, "Luis hecha", EstadoTarea.Hecha);

        var resultado = await _service.ListarAsync(new TareaFiltroDto
        {
            Estado = EstadoTarea.Hecha,
            ResponsableId = ana.Id
        });

        var unica = Assert.Single(resultado.Items);
        Assert.Equal("Ana hecha", unica.Titulo);
    }

    [Fact]
    public async Task Listar_OrdenaPorFechaLimiteAscendente()
    {
        var responsable = await _db.AddResponsableAsync();
        await _db.AddTareaAsync(responsable.Id, "Tarde", fechaLimite: Hoy.AddDays(30));
        await _db.AddTareaAsync(responsable.Id, "Pronto", fechaLimite: Hoy.AddDays(1));
        await _db.AddTareaAsync(responsable.Id, "Medio", fechaLimite: Hoy.AddDays(10));

        var resultado = await _service.ListarAsync(new TareaFiltroDto());

        Assert.Equal(new[] { "Pronto", "Medio", "Tarde" }, resultado.Items.Select(t => t.Titulo));
    }

    [Fact]
    public async Task Listar_Pagina_DevuelveElTamanoPedidoYLosTotales()
    {
        var responsable = await _db.AddResponsableAsync();
        for (var i = 1; i <= 12; i++)
            await _db.AddTareaAsync(responsable.Id, $"Tarea {i}", fechaLimite: Hoy.AddDays(i));

        var pagina1 = await _service.ListarAsync(new TareaFiltroDto { Pagina = 1, TamanoPagina = 10 });
        var pagina2 = await _service.ListarAsync(new TareaFiltroDto { Pagina = 2, TamanoPagina = 10 });

        Assert.Equal(10, pagina1.Items.Count);
        Assert.Equal(2, pagina2.Items.Count);
        Assert.Equal(12, pagina1.TotalItems);
        Assert.Equal(2, pagina1.TotalPaginas);
        Assert.Equal("Tarea 11", pagina2.Items[0].Titulo);   // sin repetir ni saltar filas
    }

    [Fact]
    public async Task Listar_PaginaFueraDeRango_DevuelveListaVacia()
    {
        var responsable = await _db.AddResponsableAsync();
        await _db.AddTareaAsync(responsable.Id);

        var resultado = await _service.ListarAsync(new TareaFiltroDto { Pagina = 5, TamanoPagina = 10 });

        Assert.Empty(resultado.Items);
        Assert.Equal(1, resultado.TotalItems);
    }
}