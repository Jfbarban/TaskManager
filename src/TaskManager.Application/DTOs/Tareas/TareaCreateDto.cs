using System.ComponentModel.DataAnnotations;
using TaskManager.Application.Validation;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tareas;

public class TareaCreateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 3,
        ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado no es válido.")]
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;

    // Nullable para que [Required] detecte cuando no se envía
    [Required(ErrorMessage = "La fecha límite es obligatoria.")]
    [FechaNoPasada]
    public DateOnly? FechaLimite { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El responsable es obligatorio.")]
    public int ResponsableId { get; set; }
}