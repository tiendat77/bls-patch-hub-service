using System.Text.Json.Serialization;

public class ResponseBase
{
    [JsonPropertyName("data")]
    public object Data { get; set; }

    [JsonPropertyName("message")]
    public string Message {get; set;}

    [JsonPropertyName("isError")]
    public bool IsError {get; set;}

    [JsonPropertyName("statusCode")]
    public int StatusCode {get; set;}

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; }
}

public class SuccessResponse : ResponseBase
{
    public SuccessResponse(
        object data,
        string message
    ) {
        Data = data;
        Message = message;
        IsError = false;
        StatusCode = 200;
    }
}

public class ErrorResponse : ResponseBase
{
    public ErrorResponse(
        object data,
        string message,
        string errorCode = ""
    ) {
        Data = data;
        Message = message;
        IsError = true;
        StatusCode = 400;
        ErrorCode = errorCode;
    }
}

[JsonSerializable(typeof(ResponseBase))]
internal partial class ResponseBaseContext : JsonSerializerContext {}
