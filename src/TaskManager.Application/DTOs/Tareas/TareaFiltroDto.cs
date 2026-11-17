using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tareas;

public class TareaFiltroDto
{
    [EnumDataType(typeof(EstadoTarea), ErrorMessage = "El estado no es válido.")]
    public EstadoTarea? Estado { get; set; }

    public int? ResponsableId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La página debe ser 1 o mayor.")]
    public int Pagina { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "El tamaño de página debe estar entre 1 y 50.")]
    public int TamanoPagina { get; set; } = 10;
}