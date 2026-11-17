using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Domain.Enums;
using TaskManager.Web.Helpers;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

/// <summary>Base común de las páginas Crear y Editar.</summary>
public abstract class TareaFormPageModel : PageModel
{
    private static readonly string[] Campos =
    {
        nameof(TareaFormModel.Titulo),
        nameof(TareaFormModel.Descripcion),
        nameof(TareaFormModel.Estado),
        nameof(TareaFormModel.FechaLimite),
        nameof(TareaFormModel.ResponsableId)
    };

    protected readonly IApiClient Api;

    protected TareaFormPageModel(IApiClient api) => Api = api;

    [BindProperty] public TareaFormModel Tarea { get; set; } = new();

    public abstract string TextoBoton { get; }

    public IReadOnlyList<SelectListItem> EstadosItems { get; } = Enum.GetValues<EstadoTarea>()
        .Select(e => new SelectListItem(e.Texto(), e.ToString()))
        .ToList();

    public IReadOnlyList<SelectListItem> ResponsablesItems { get; private set; } = Array.Empty<SelectListItem>();

    public string? ErrorCarga { get; private set; }

    public bool HayResponsables => ResponsablesItems.Count > 0;

    protected async Task CargarResponsablesAsync(CancellationToken ct)
    {
        try
        {
            var responsables = await Api.ListarResponsablesAsync(ct);
            ResponsablesItems = responsables
                .Select(r => new SelectListItem(r.Nombre, r.Id.ToString()))
                .ToList();
        }
        catch (ApiException ex)
        {
            ErrorCarga = $"No se pudo cargar la lista de responsables. {ex.Message}";
        }
    }

    /// <summary>Vuelve a mostrar el formulario (con sus errores) recargando los desplegables.</summary>
    protected async Task<IActionResult> RecargarFormularioAsync(CancellationToken ct)
    {
        await CargarResponsablesAsync(ct);
        return Page();
    }

    protected void MostrarErrorDeApi(ApiException ex) =>
        ModelState.AgregarErroresDeApi(ex, nameof(Tarea), Campos);
}