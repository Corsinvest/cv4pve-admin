namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Models;

/// <summary>
/// Represents an ORDER BY clause
/// </summary>
public class OrderBy
{
    public string Field { get; set; } = default!;
    public string Direction { get; set; } = "asc";

    public OrderBy Clone()
        => new()
        {
            Field = Field,
            Direction = Direction
        };
}
