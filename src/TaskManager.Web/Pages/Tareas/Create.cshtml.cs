using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

public class CreateModel : TareaFormPageModel
{
    public CreateModel(IApiClient api) : base(api) { }

    public override string TextoBoton => "Crear tarea";

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        // Sugerimos una semana de plazo; el usuario puede cambiarlo.
        Tarea.FechaLimite = DateOnly.FromDateTime(DateTime.Today).AddDays(7);
        return await RecargarFormularioAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return await RecargarFormularioAsync(ct);

        try
        {
            await Api.CrearTareaAsync(Tarea.ToCreateDto(), ct);
        }
        catch (ApiException ex)
        {
            MostrarErrorDeApi(ex);
            return await RecargarFormularioAsync(ct);
        }

        TempData["Exito"] = "Tarea creada correctamente.";
        return RedirectToPage("Index");
    }
}