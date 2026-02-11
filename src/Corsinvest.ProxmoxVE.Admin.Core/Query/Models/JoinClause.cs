namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents a JOIN clause between two tables
/// </summary>
public class JoinClause
{
    public string Table { get; set; } = default!;
    public string ForeignKey { get; set; } = default!;
    public string PrimaryKey { get; set; } = default!;

    public JoinClause Clone()
        => new()
        {
            Table = Table,
            ForeignKey = ForeignKey,
            PrimaryKey = PrimaryKey
        };
}
