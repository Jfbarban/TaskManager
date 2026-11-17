namespace TaskManager.Application.Exceptions;

/// <summary>La operación choca con el estado actual de los datos → 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}