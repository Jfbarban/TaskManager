using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

public class EditModel : TareaFormPageModel
{
    public EditModel(IApiClient api) : base(api) { }

    public override string TextoBoton => "Guardar cambios";

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            Tarea = TareaFormModel.From(await Api.ObtenerTareaAsync(id, ct));
        }
        catch (ApiNotFoundException)
        {
            TempData["Error"] = "La tarea que intentas editar no existe o ya fue eliminada.";
            return RedirectToPage("Index");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = $"No se pudo cargar la tarea. {ex.Message}";
            return RedirectToPage("Index");
        }

        return await RecargarFormularioAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return await RecargarFormularioAsync(ct);

        try
        {
            await Api.ActualizarTareaAsync(id, Tarea.ToUpdateDto(), ct);
        }
        catch (ApiNotFoundException)
        {
            TempData["Error"] = "La tarea que intentas editar ya no existe.";
            return RedirectToPage("Index");
        }
        catch (ApiException ex)
        {
            MostrarErrorDeApi(ex);
            return await RecargarFormularioAsync(ct);
        }

        TempData["Exito"] = "Tarea actualizada correctamente.";
        return RedirectToPage("Index");
    }
}