using System.ComponentModel.DataAnnotations;
using TaskManager.Application.DTOs.Tareas;
using TaskManager.Domain.Enums;

namespace TaskManager.Web.Pages.Tareas;

/// <summary>
/// Datos del formulario de tarea (crear y editar). La regla "no fecha pasada al crear"
/// no se duplica aquí: la valida la API y su error vuelve al campo.
/// </summary>
public class TareaFormModel
{
    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [Display(Name = "Estado")]
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    [Display(Name = "Fecha límite")]
    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    public DateOnly? FechaLimite { get; set; }

    [Display(Name = "Responsable")]
    [Required(ErrorMessage = "Selecciona un responsable.")]
    public int? ResponsableId { get; set; }

    public TareaCreateDto ToCreateDto() => new()
    {
        Titulo = Titulo,
        Descripcion = Descripcion,
        Estado = Estado,
        FechaLimite = FechaLimite,
        ResponsableId = ResponsableId ?? 0
    };

    public TareaUpdateDto ToUpdateDto() => new()
    {
        Titulo = Titulo,
        Descripcion = Descripcion,
        Estado = Estado,
        FechaLimite = FechaLimite,
        ResponsableId = ResponsableId ?? 0
    };

    public static TareaFormModel From(TareaDto tarea) => new()
    {
        Titulo = tarea.Titulo,
        Descripcion = tarea.Descripcion,
        Estado = tarea.Estado,
        FechaLimite = tarea.FechaLimite,
        ResponsableId = tarea.ResponsableId
    };
}