namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;

public record CommandResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static CommandResult Success()
        => new()
        {
            IsSuccess = true
        };

    public static CommandResult Failure(string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}

public record CommandResult<T> : CommandResult
{
    public T? Data { get; init; }

    public static CommandResult<T> Success(T data)
        => new()
        {
            IsSuccess = true,
            Data = data
        };

    public static new CommandResult<T> Failure(string errorMessage)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}
