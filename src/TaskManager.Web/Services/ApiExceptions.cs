using System.Net;

namespace TaskManager.Web.Services;

public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public ApiException(string message, HttpStatusCode? statusCode = null, Exception? inner = null)
        : base(message, inner) => StatusCode = statusCode;
}

/// <summary>La API respondió 404.</summary>
public class ApiNotFoundException : ApiException
{
    public ApiNotFoundException(string message) : base(message, HttpStatusCode.NotFound) { }
}

/// <summary>La API respondió 409.</summary>
public class ApiConflictException : ApiException
{
    public ApiConflictException(string message) : base(message, HttpStatusCode.Conflict) { }
}

/// <summary>La API respondió 400 con el detalle por campo.</summary>
public class ApiValidationException : ApiException
{
    public IDictionary<string, string[]> Errors { get; }

    public ApiValidationException(string message, IDictionary<string, string[]> errors)
        : base(message, HttpStatusCode.BadRequest) => Errors = errors;
}