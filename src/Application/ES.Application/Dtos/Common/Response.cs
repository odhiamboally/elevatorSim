using MessagePack;
using System.Text.Json.Serialization;

namespace ES.Application.Dtos.Common;

[MessagePackObject]
public record Response<T>
{
    public bool Successful { get; init; } = true;
    public string? Message { get; init; } = "Operation Successful";
    public T? Data { get; init; }
    public Exception? Exception { get; init; }

    // To hold any extra information
    public Dictionary<string, object>? AdditionalData { get; init; } = new Dictionary<string, object>();


    [JsonConstructor]
    private Response(bool successful, string? message, T? data, Exception? exception, Dictionary<string, object>? additionalData)
    {
        Successful = successful;
        Message = message;
        Data = data;
        Exception = exception;
        AdditionalData = additionalData;
    }

    public static Response<T> Success(string message, T value, Exception? exception = null, Dictionary<string, object>? additionalData = null)
    {
        return new Response<T>(true, message, value, null, additionalData);
    }

    public static Response<T> Failure(string errorMessage, T? data = default, Exception? error = null, Dictionary<string, object>? additionalData = null)
    {
        return new Response<T>(false, errorMessage, data, error, additionalData);
    }

}
