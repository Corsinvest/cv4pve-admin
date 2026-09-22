/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

public abstract class NotifierConfiguration : IName, IEnabled
{
    [Required] public string Name { get; set; } = default!;
    public bool Enabled { get; set; } = true;

    public async Task<OperationResult> SendTestAsync(string appName, IServiceProvider serviceProvider)
    {
        var testContent = "This is a sample text attachment.\nUse this to test notification attachments."u8.ToArray();
        await using var stream = new MemoryStream(testContent);

        // Names the configuration that sent it: with several set up, an arriving test message
        // otherwise says nothing about which one works.
        return await SendImpAsync(new NotifierMessage
        {
            Subject = $"{appName} - Test notification",
            Body = $"This is a test notification from '{Name}'. "
                   + "If it arrived with its attachment, the configuration works.",
            Severity = NotifierMessageSeverity.Success,
            Attachments = [new(stream, "Test.txt", "text/plain")]
        }, serviceProvider);
    }

    public async Task<OperationResult> SendAsync(NotifierMessage mailMessage, IServiceProvider serviceProvider)
    {
        var result = OperationResult.Ok();
        if (Enabled)
        {
            try
            {
                result = await SendImpAsync(mailMessage, serviceProvider);
            }
            catch (Exception ex)
            {
                result = OperationResult.Fail(ex.Message);
            }
        }
        return result;
    }

    protected abstract Task<OperationResult> SendImpAsync(NotifierMessage message, IServiceProvider serviceProvider);
}
