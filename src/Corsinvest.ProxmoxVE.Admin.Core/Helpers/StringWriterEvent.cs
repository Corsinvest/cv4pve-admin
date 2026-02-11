namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public class StringWriterEvent : StringWriter
{
    public event EventHandler<string> WritedData = default!;
    private void InvokeWriteData(string value) => WritedData?.Invoke(this, value ?? string.Empty);

    public override void Write(char value)
    {
        base.Write(value);
        InvokeWriteData(value.ToString());
    }

    public override void Write(string? value)
    {
        base.Write(value);
        InvokeWriteData(value!);
    }

    public override void WriteLine()
    {
        base.WriteLine();
        InvokeWriteData(Environment.NewLine);
    }
}
