using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

public class DetailsModel : PageModel
{
    private readonly IApiClient _api;

    public DetailsModel(IApiClient api) => _api = api;

    public TareaDto Tarea { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            Tarea = await _api.ObtenerTareaAsync(id, ct);
            return Page();
        }
        catch (ApiNotFoundException)
        {
            TempData["Error"] = "La tarea no existe o ya fue eliminada.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"No se pudo cargar la tarea. {ex.Message}";
        }

        return RedirectToPage("Index");
    }
}