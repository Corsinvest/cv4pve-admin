namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

/// <summary>
/// Defines a search filter (e.g., "vm:", "ct:", "node:")
/// </summary>
public record SearchFilter(
    string Id,
    string Label,
    string Icon,
    BadgeStyle BadgeStyle)
{
    /// <summary>
    /// Prefix for the filter (e.g., "vm:", "ct:")
    /// </summary>
    public string Prefix => $"{Id}:";
}
