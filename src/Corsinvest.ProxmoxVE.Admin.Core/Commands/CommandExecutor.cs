/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Commands;

public class CommandExecutor(IServiceProvider serviceProvider)
{
    public async Task<TResult> ExecuteAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for command type '{commandType.Name}'. " +
                $"Expected handler: '{handlerType.Name}'");

        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<,>.HandleAsync))
            ?? throw new InvalidOperationException($"HandleAsync method not found on handler type '{handlerType.Name}'");

        var task = handleMethod.Invoke(handler, [command, cancellationToken]) as Task<TResult>
            ?? throw new InvalidOperationException($"Handler did not return expected Task<{typeof(TResult).Name}>");

        return await task;
    }
}
