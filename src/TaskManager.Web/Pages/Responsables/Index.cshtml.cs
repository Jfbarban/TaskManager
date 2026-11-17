using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Web.Helpers;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Responsables;

public class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public IndexModel(IApiClient api) => _api = api;

    [BindProperty] public ResponsableCreateDto Nuevo { get; set; } = new();

    public IReadOnlyList<ResponsableDto> Responsables { get; private set; } = Array.Empty<ResponsableDto>();
    public string? ErrorApi { get; private set; }

    public async Task OnGetAsync(CancellationToken ct) => await CargarAsync(ct);

    // Añadir (formulario superior)
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarAsync(ct);
            return Page();
        }

        try
        {
            var creado = await _api.CrearResponsableAsync(Nuevo, ct);
            TempData["Exito"] = $"Responsable «{creado.Nombre}» añadido correctamente.";
            return RedirectToPage();
        }
        catch (ApiException ex)
        {
            ModelState.AgregarErroresDeApi(ex, nameof(Nuevo), new[] { nameof(ResponsableCreateDto.Nombre) });
            await CargarAsync(ct);
            return Page();
        }
    }

    // Eliminar (formulario de la ventana modal). Este formulario no envía "Nuevo",
    // así que aquí no se mira el ModelState.
    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        try
        {
            await _api.EliminarResponsableAsync(id, ct);
            TempData["Exito"] = "Responsable eliminado correctamente.";
        }
        catch (ApiConflictException ex)
        {
            // 409: tiene tareas asignadas. Mostramos el mensaje de la API tal cual.
            TempData["Error"] = ex.Message;
        }
        catch (ApiNotFoundException)
        {
            TempData["Error"] = "El responsable ya no existe.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"No se pudo eliminar el responsable. {ex.Message}";
        }

        return RedirectToPage();
    }

    private async Task CargarAsync(CancellationToken ct)
    {
        try
        {
            Responsables = await _api.ListarResponsablesAsync(ct);
        }
        catch (ApiException ex)
        {
            ErrorApi = ex.Message;
        }
    }
}