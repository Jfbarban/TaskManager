using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class Tarea
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;
    public DateOnly FechaLimite { get; set; }

    public int ResponsableId { get; set; }
    public Responsable Responsable { get; set; } = null!;
}