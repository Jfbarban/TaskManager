using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Services;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Services;

public class ResponsableServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly ResponsableService _service;

    public ResponsableServiceTests() => _service = new ResponsableService(_db.Context);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Crear_GuardaElResponsableYRecortaEspacios()
    {
        var resultado = await _service.CrearAsync(new ResponsableCreateDto { Nombre = "  Pedro  " });

        Assert.True(resultado.Id > 0);
        Assert.Equal("Pedro", resultado.Nombre);
    }

    [Fact]
    public async Task Listar_DevuelveLosResponsablesOrdenadosPorNombre()
    {
        await _db.AddResponsableAsync("Zoe");
        await _db.AddResponsableAsync("Ana");
        await _db.AddResponsableAsync("Marta");

        var resultado = await _service.ListarAsync();

        Assert.Equal(new[] { "Ana", "Marta", "Zoe" }, resultado.Select(r => r.Nombre));
    }

    [Fact]
    public async Task Obtener_Inexistente_LanzaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerAsync(999));
    }

    [Fact]
    public async Task Eliminar_SinTareas_LoElimina()
    {
        var responsable = await _db.AddResponsableAsync("Sin tareas");

        await _service.EliminarAsync(responsable.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObtenerAsync(responsable.Id));
    }

    [Fact]
    public async Task Eliminar_ConTareasAsignadas_LanzaConflictExceptionYNoLoBorra()
    {
        var responsable = await _db.AddResponsableAsync("Ocupado");
        await _db.AddTareaAsync(responsable.Id);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.EliminarAsync(responsable.Id));

        Assert.Contains("Ocupado", ex.Message);
        var sigueExistiendo = await _service.ObtenerAsync(responsable.Id);
        Assert.Equal("Ocupado", sigueExistiendo.Nombre);
    }

    [Fact]
    public async Task Eliminar_Inexistente_LanzaNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.EliminarAsync(999));
    }
}