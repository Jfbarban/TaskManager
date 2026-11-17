using TaskManager.Domain.Enums;

namespace TaskManager.Web.Helpers;

public record PaginacionVm(int Pagina, int TotalPaginas, EstadoTarea? Estado, int? ResponsableId);