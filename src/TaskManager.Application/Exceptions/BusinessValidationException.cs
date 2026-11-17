namespace TaskManager.Application.Exceptions;

/// <summary>Regla de validación que necesita la BD (ej. responsable inexistente) → 400.</summary>
public class BusinessValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public BusinessValidationException(string campo, string mensaje) : base(mensaje)
    {
        Errors = new Dictionary<string, string[]> { [campo] = [mensaje] };
    }
}