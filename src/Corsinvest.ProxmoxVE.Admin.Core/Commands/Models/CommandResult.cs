/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Commands.Models;

public record CommandResult
{
    public CommandResultStatus Status { get; init; } = CommandResultStatus.Failed;

    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status == CommandResultStatus.Success;

    public bool IsPermissionDenied => Status is CommandResultStatus.Forbidden or CommandResultStatus.Unauthorized;

    public static CommandResult Success()
        => new() { Status = CommandResultStatus.Success };

    public static CommandResult Failure(string errorMessage)
        => new() { Status = CommandResultStatus.Failed, ErrorMessage = errorMessage };

    public static CommandResult Forbidden(string errorMessage = "Operation not permitted")
        => new() { Status = CommandResultStatus.Forbidden, ErrorMessage = errorMessage };

    public static CommandResult Unauthorized(string errorMessage = "Authentication required")
        => new() { Status = CommandResultStatus.Unauthorized, ErrorMessage = errorMessage };
}

public record CommandResult<T> : CommandResult
{
    public T? Data { get; init; }

    public static CommandResult<T> Success(T data)
        => new() { Status = CommandResultStatus.Success, Data = data };

    public static new CommandResult<T> Failure(string errorMessage)
        => new() { Status = CommandResultStatus.Failed, ErrorMessage = errorMessage };

    public static new CommandResult<T> Forbidden(string errorMessage = "Operation not permitted")
        => new() { Status = CommandResultStatus.Forbidden, ErrorMessage = errorMessage };

    public static new CommandResult<T> Unauthorized(string errorMessage = "Authentication required")
        => new() { Status = CommandResultStatus.Unauthorized, ErrorMessage = errorMessage };
}
