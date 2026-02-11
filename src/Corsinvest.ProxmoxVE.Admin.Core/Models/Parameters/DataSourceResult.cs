namespace Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

public record DataSourceResult(IEnumerable<object> Data,
                               string TextProperty,
                               string ValueProperty,
                               IReadOnlyList<DataSourceColumn>? Columns = null)
{
    public static DataSourceResult Empty = new([], "Id", "Id");
}
