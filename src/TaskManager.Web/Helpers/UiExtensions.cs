using System.Globalization;
using TaskManager.Domain.Enums;

namespace TaskManager.Web.Helpers;

public static class UiExtensions
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public static string Texto(this EstadoTarea estado) => estado switch
    {
        EstadoTarea.Pendiente => "Pendiente",
        EstadoTarea.EnCurso => "En curso",
        EstadoTarea.Hecha => "Hecha",
        _ => estado.ToString()
    };

    /// <summary>Sufijo de la clase CSS: pendiente, encurso, hecha.</summary>
    public static string Css(this EstadoTarea estado) => estado.ToString().ToLowerInvariant();

    public static string Icono(this EstadoTarea estado) => estado switch
    {
        EstadoTarea.Pendiente => "bi-hourglass-split",
        EstadoTarea.EnCurso => "bi-play-circle-fill",
        EstadoTarea.Hecha => "bi-check-circle-fill",
        _ => "bi-circle"
    };

    public static string Corta(this DateOnly fecha) => fecha.ToString("d MMM yyyy", Es);

    public static string Iniciales(this string nombre)
    {
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..1].ToUpperInvariant(),
            _ => (partes[0][..1] + partes[^1][..1]).ToUpperInvariant()
        };
    }

    /// <summary>Matiz (0-359) estable por nombre, para colorear el avatar.</summary>
    public static int Tono(this string nombre)
    {
        var hash = nombre.Aggregate(7u, (acc, c) => unchecked(acc * 31 + c));
        return (int)(hash % 360);
    }
}