namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class FormatterHelper
{
    //public const string FormatBytes = "{0:" + FormatHelper.DataFormatBytes + "}";
    //public const string FormatUnixTime = "{0:" + FormatHelper.FormatUptimeUnixTime + "}";

    private static readonly Lazy<PveFormatProvider> _formatProvider = new();
    public static PveFormatProvider FormatProvider { get; } = _formatProvider.Value;
}
