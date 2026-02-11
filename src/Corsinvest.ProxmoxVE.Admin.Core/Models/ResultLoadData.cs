namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public record ResultLoadData<TResult>(List<TResult> Data, int TotalCount, string? Filter);
