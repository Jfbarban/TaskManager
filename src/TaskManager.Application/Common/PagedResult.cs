namespace TaskManager.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalItems { get; set; }
    public int TotalPaginas => TamanoPagina == 0
        ? 0
        : (int)Math.Ceiling(TotalItems / (double)TamanoPagina);
}