using System.ComponentModel.DataAnnotations;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Domain.Enums;

namespace TaskManager.Tests.Validation;

public class DtoValidationTests
{
    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    private static List<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
        return resultados;
    }

    private static void AssertError(List<ValidationResult> resultados, string campo) =>
        Assert.Contains(resultados, r => r.MemberNames.Contains(campo));

    private static TareaCreateDto CreateValido() => new()
    {
        Titulo = "Tarea válida",
        Descripcion = "Descripción",
        Estado = EstadoTarea.Pendiente,
        FechaLimite = Hoy.AddDays(1),
        ResponsableId = 1
    };

    // ---------- TareaCreateDto ----------

    [Fact]
    public void TareaCreate_ConDatosValidos_NoTieneErrores()
    {
        Assert.Empty(Validar(CreateValido()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab")]
    public void TareaCreate_TituloInvalido_DaError(string titulo)
    {
        var dto = CreateValido();
        dto.Titulo = titulo;

        AssertError(Validar(dto), nameof(TareaCreateDto.Titulo));
    }

    [Fact]
    public void TareaCreate_TituloDemasiadoLargo_DaError()
    {
        var dto = CreateValido();
        dto.Titulo = new string('a', 201);

        AssertError(Validar(dto), nameof(TareaCreateDto.Titulo));
    }

    [Fact]
    public void TareaCreate_DescripcionDemasiadoLarga_DaError()
    {
        var dto = CreateValido();
        dto.Descripcion = new string('a', 1001);

        AssertError(Validar(dto), nameof(TareaCreateDto.Descripcion));
    }

    [Fact]
    public void TareaCreate_FechaEnElPasado_DaError()
    {
        var dto = CreateValido();
        dto.FechaLimite = Hoy.AddDays(-1);

        AssertError(Validar(dto), nameof(TareaCreateDto.FechaLimite));
    }

    [Fact]
    public void TareaCreate_FechaDeHoy_EsValida()
    {
        var dto = CreateValido();
        dto.FechaLimite = Hoy;

        Assert.Empty(Validar(dto));
    }

    [Fact]
    public void TareaCreate_SinFecha_DaError()
    {
        var dto = CreateValido();
        dto.FechaLimite = null;

        AssertError(Validar(dto), nameof(TareaCreateDto.FechaLimite));
    }

    [Fact]
    public void TareaCreate_EstadoFueraDeRango_DaError()
    {
        var dto = CreateValido();
        dto.Estado = (EstadoTarea)7;

        AssertError(Validar(dto), nameof(TareaCreateDto.Estado));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void TareaCreate_ResponsableIdInvalido_DaError(int responsableId)
    {
        var dto = CreateValido();
        dto.ResponsableId = responsableId;

        AssertError(Validar(dto), nameof(TareaCreateDto.ResponsableId));
    }

    // ---------- TareaUpdateDto ----------

    [Fact]
    public void TareaUpdate_FechaEnElPasado_EsValida()
    {
        var dto = new TareaUpdateDto
        {
            Titulo = "Ya vencida",
            Estado = EstadoTarea.Hecha,
            FechaLimite = Hoy.AddDays(-10),
            ResponsableId = 1
        };

        Assert.Empty(Validar(dto));
    }

    // ---------- TareaFiltroDto ----------

    [Fact]
    public void Filtro_PorDefecto_EsValidoYUsaPagina1Con10Elementos()
    {
        var filtro = new TareaFiltroDto();

        Assert.Empty(Validar(filtro));
        Assert.Equal(1, filtro.Pagina);
        Assert.Equal(10, filtro.TamanoPagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Filtro_PaginaInvalida_DaError(int pagina)
    {
        AssertError(Validar(new TareaFiltroDto { Pagina = pagina }), nameof(TareaFiltroDto.Pagina));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void Filtro_TamanoPaginaFueraDeRango_DaError(int tamano)
    {
        AssertError(Validar(new TareaFiltroDto { TamanoPagina = tamano }), nameof(TareaFiltroDto.TamanoPagina));
    }

    // ---------- ResponsableCreateDto ----------

    [Fact]
    public void ResponsableCreate_ConNombreValido_NoTieneErrores()
    {
        Assert.Empty(Validar(new ResponsableCreateDto { Nombre = "Pedro" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void ResponsableCreate_NombreInvalido_DaError(string nombre)
    {
        AssertError(Validar(new ResponsableCreateDto { Nombre = nombre }), nameof(ResponsableCreateDto.Nombre));
    }

    [Fact]
    public void ResponsableCreate_NombreDemasiadoLargo_DaError()
    {
        var dto = new ResponsableCreateDto { Nombre = new string('a', 101) };

        AssertError(Validar(dto), nameof(ResponsableCreateDto.Nombre));
    }
}