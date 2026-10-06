namespace Arrapa.ClinicOS.Shared.Results;

/// <summary>
/// Represents a domain or application error with a stable machine code and human message.
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public IReadOnlyDictionary<string, string[]>? Details { get; }

    private Error(string code, string message, ErrorType type, IReadOnlyDictionary<string, string[]>? details = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Details = details;
    }

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? details = null) => new(code, message, ErrorType.Validation, details);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6,
    Unexpected = 7
}
