using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public class ResourceTagStyleColumn<TItem> : ResourceColumn<TItem> where TItem : notnull
{
    [Parameter] public string? TagStyleColorMap { get; set; }

    protected override void OnInitialized()
    {
        Property = nameof(ClusterResource.Tags);

        base.OnInitialized();

        Template = item => builder =>
        {
            if (item != null)
            {
                var ret = PveAdminUIHelper.GetTagsStyle(GetValue(item) + string.Empty, TagStyleColorMap!);
                builder.AddContent(0, (MarkupString)ret.JoinAsString("<br />"));
            }
        };
    }
}
