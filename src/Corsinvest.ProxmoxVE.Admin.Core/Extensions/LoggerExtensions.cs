/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class LoggerExtensions
{
    private class Operation : IDisposable
    {
        private readonly Stopwatch _timer;
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;
        private readonly string _messageTemplate;
        private readonly object?[] _args;
        private readonly bool _startAndEnd;

        public Operation(ILogger logger, LogLevel logLevel, bool startAndEnd, string messageTemplate, params object?[] args)
        {
            _logger = logger;
            _logLevel = logLevel;
            _messageTemplate = messageTemplate;
            _args = args;
            _startAndEnd = startAndEnd;

            if (_startAndEnd) { _logger.Log(_logLevel, "* Start " + _messageTemplate, _args); }

            _timer = new Stopwatch();
            _timer.Start();
        }

        public void Dispose()
        {
            _timer.Stop();

            var message = _messageTemplate;
            if (_startAndEnd) { message = "* End " + message; }

            // Avoid array allocation in hot path when no args
            if (_args.Length == 0)
            {
                _logger.Log(_logLevel, message + " Execution {Execution}", _timer.Elapsed);
            }
            else
            {
                _logger.Log(_logLevel, message + " Execution {Execution}", [.. _args, _timer.Elapsed]);
            }
        }
    }

    public static IDisposable LogTimeOperation(this ILogger logger, LogLevel logLevel, bool startEnd, string messageTemplate, params object?[] args)
        => new Operation(logger, logLevel, startEnd, messageTemplate, args);
}
