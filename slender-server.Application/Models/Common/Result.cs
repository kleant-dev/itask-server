namespace slender_server.Application.Models.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ErrorType ErrorType { get; set; }
    public Dictionary<string, string>? Errors { get; }

    private Result(bool isSuccess, T? value, string? error,ErrorType errorType, Dictionary<string, string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = error;
        Errors = errors;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null,ErrorType.None);
    public static Result<T> Failure(string error,ErrorType errorType) => new(false, default, error,errorType);
    public static Result<T> Failure(Dictionary<string, string> errors,ErrorType errorType) => new(false, default, null,errorType, errors);
}

public enum ErrorType
{
    None,
    NotFound,
    Validation,
    Conflict,
    Unauthorized,
    Forbidden,
    InternalError
}

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public ErrorType ErrorType { get; set; }

    private Result(bool isSuccess, string? error,ErrorType errorType)
    {
        IsSuccess = isSuccess;
        ErrorMessage = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null,ErrorType.None);
    public static Result Failure(string error,ErrorType errorType) => new(false, error,errorType);
}