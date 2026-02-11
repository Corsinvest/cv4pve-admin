namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents a SQL-like query for data sources
/// </summary>
public class Query
{
    public IEnumerable<string> Select { get; set; } = [];
    public string From { get; set; } = default!;
    public IEnumerable<JoinClause> Join { get; set; } = [];
    public WhereClause Where { get; set; } = new();
    public IEnumerable<string> GroupBy { get; set; } = [];
    public IEnumerable<OrderBy> OrderBy { get; set; } = [];
    public int Offset { get; set; }
    public int Limit { get; set; }

    public Query Clone()
        => new()
        {
            Select = [.. Select],
            From = From,
            GroupBy = [.. GroupBy],
            Join = [.. Join.Select(j => j.Clone())],
            Where = Where.Clone(),
            OrderBy = [.. OrderBy.Select(o => o.Clone())],
            Offset = Offset,
            Limit = Limit
        };
}
