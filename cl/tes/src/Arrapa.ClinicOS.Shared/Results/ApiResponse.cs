namespace Arrapa.ClinicOS.Shared.Results;

/// <summary>
/// Consistent HTTP response envelope returned by every API endpoint.
/// </summary>
public sealed class ApiResponse<TData>
{
    public bool Success { get; init; }
    public TData? Data { get; init; }
    public string? Message { get; init; }
    public ApiError? Error { get; init; }
    public string? CorrelationId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public static ApiResponse<TData> Ok(TData data, string? message = null, string? correlationId = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        CorrelationId = correlationId
    };

    public static ApiResponse<TData> Fail(Error error, string? correlationId = null) => new()
    {
        Success = false,
        Error = ApiError.From(error),
        Message = error.Message,
        CorrelationId = correlationId
    };
}

public sealed class ApiError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
    public IReadOnlyDictionary<string, string[]>? Details { get; init; }

    public static ApiError From(Error error) => new()
    {
        Code = error.Code,
        Message = error.Message,
        Type = error.Type.ToString(),
        Details = error.Details
    };
}
