using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs.Responsables;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Domain.Enums;
using TaskManager.Web.Services;

namespace TaskManager.Web.Pages.Tareas;

public class IndexModel : PageModel
{
    // Nueve tarjetas por página encajan en la cuadrícula de 3 columnas.
    private const int TamanoPagina = 9;

    private readonly IApiClient _api;

    public IndexModel(IApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)] public EstadoTarea? Estado { get; set; }
    [BindProperty(SupportsGet = true)] public int? ResponsableId { get; set; }
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public PagedResult<TareaDto> Resultado { get; private set; } = new();
    public IReadOnlyList<ResponsableDto> Responsables { get; private set; } = Array.Empty<ResponsableDto>();
    public string? ErrorApi { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (Pagina < 1) Pagina = 1;

        try
        {
            Responsables = await _api.ListarResponsablesAsync(ct);
            Resultado = await _api.ListarTareasAsync(new TareaFiltroDto
            {
                Estado = Estado,
                ResponsableId = ResponsableId,
                Pagina = Pagina,
                TamanoPagina = TamanoPagina
            }, ct);

            // Si la página pedida ya no existe (por ejemplo, tras borrar), vamos a la última.
            if (Resultado.Items.Count == 0 && Resultado.TotalItems > 0 && Pagina > 1)
                return RedirectToPage(new { Pagina = Resultado.TotalPaginas, Estado, ResponsableId });
        }
        catch (ApiException ex)
        {
            ErrorApi = ex.Message;
        }

        return Page();
    }
}