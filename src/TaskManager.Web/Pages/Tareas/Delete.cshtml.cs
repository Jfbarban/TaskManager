using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

public class DeleteModel : PageModel
{
    private readonly IApiClient _api;

    public DeleteModel(IApiClient api) => _api = api;

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

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        try
        {
            await _api.EliminarTareaAsync(id, ct);
            TempData["Exito"] = "Tarea eliminada correctamente.";
        }
        catch (ApiNotFoundException)
        {
            TempData["Error"] = "La tarea ya no existe.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"No se pudo eliminar la tarea. {ex.Message}";
        }

        return RedirectToPage("Index");
    }
}