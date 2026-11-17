using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManager.Web.Services;

namespace TaskManager.Web.Helpers;

public static class ModelStateExtensions
{
    /// <summary>
    /// Pasa los errores de la API al ModelState: los de un campo conocido se asocian a ese campo
    /// y el resto (incluidos 404, 409 o fallos de conexión) quedan como error general del formulario.
    /// </summary>
    public static void AgregarErroresDeApi(
        this ModelStateDictionary modelState,
        ApiException excepcion,
        string prefijo,
        IEnumerable<string> camposConocidos)
    {
        if (excepcion is ApiValidationException validacion && validacion.Errors.Count > 0)
        {
            var conocidos = new HashSet<string>(camposConocidos, StringComparer.OrdinalIgnoreCase);

            foreach (var (campo, mensajes) in validacion.Errors)
                foreach (var mensaje in mensajes)
                    modelState.AddModelError(conocidos.Contains(campo) ? $"{prefijo}.{campo}" : string.Empty, mensaje);
        }
        else
        {
            modelState.AddModelError(string.Empty, excepcion.Message);
        }
    }
}