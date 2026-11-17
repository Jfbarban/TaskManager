using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tareas;

public class TareaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoTarea Estado { get; set; }
    public DateOnly FechaLimite { get; set; }
    public int ResponsableId { get; set; }
    public string ResponsableNombre { get; set; } = string.Empty;
}