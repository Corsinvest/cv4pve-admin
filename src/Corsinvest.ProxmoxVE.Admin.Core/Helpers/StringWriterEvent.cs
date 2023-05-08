/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public class StringWriterEvent : StringWriter
{
    public event EventHandler<string> WritedData = default!;
    private void InvokeWriteData(string value) => WritedData?.Invoke(this, value);

    public override void Write(char value)
    {
        base.Write(value);
        InvokeWriteData(value + "");
    }

    public override void Write(string? value)
    {
        base.Write(value);
        InvokeWriteData(value + "");
    }

    public override void WriteLine()
    {
        base.WriteLine();
        InvokeWriteData(Environment.NewLine);
    }
}