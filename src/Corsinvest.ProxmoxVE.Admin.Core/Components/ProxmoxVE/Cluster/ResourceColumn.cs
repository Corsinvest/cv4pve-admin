using Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Microsoft.AspNetCore.Components.Rendering;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

public class ResourceColumn<TItem> : BaseColumn<TItem> where TItem : notnull
{
    [Parameter] public ResourceColumnIconStatus ShowIconStatus { get; set; } = ResourceColumnIconStatus.None;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var isStatus = Property == nameof(ClusterResource.Status);

        FilterMode = Property switch
        {
            nameof(ClusterResource.Node)
                or nameof(ClusterResource.Type)
                or nameof(ClusterResource.Status) => Radzen.FilterMode.CheckBoxList,
            _ => null
        };

        if (Property is nameof(ClusterResource.Type) or nameof(ClusterResource.Status))
        {
            //            if (string.IsNullOrEmpty(Width)) { Width = "105px"; }
        }

        if (!string.IsNullOrEmpty(Property))
        {
            if (isStatus)
            {
                //if (string.IsNullOrEmpty(Width)) { Width = "80px"; }
                //Filterable = false;
                //Sortable = false;
            }
            //else if (UseProgressBarPercentage)
            //{
            //}
            else
            {
                if (!string.IsNullOrEmpty(FormatString)) { FormatProvider ??= FormatterHelper.FormatProvider; }
            }

            var baseTemplate = Template!;

            Template = item => builder =>
            {
                if (item != null)
                {
                    void AddIcon(RenderTreeBuilder builder1, int index)
                    {
                        var resource = (item as ClusterResource)!;
                        builder1.OpenComponent<IconStatusResource>(index);
                        builder1.AddAttribute(++index, nameof(IconStatusResource.Status), resource.Status);
                        builder1.AddAttribute(++index, nameof(IconStatusResource.Type), resource.Type);
                        builder1.AddAttribute(++index, nameof(IconStatusResource.Locked), resource.IsLocked);
                        builder1.CloseComponent();
                    }

                    switch (ShowIconStatus)
                    {
                        case ResourceColumnIconStatus.None: baseTemplate!(item)(builder); break;
                        case ResourceColumnIconStatus.Icon: AddIcon(builder, 0); break;

                        case ResourceColumnIconStatus.IconAndText:
                            builder.OpenComponent<RadzenStack>(0);
                            builder.AddAttribute(1, nameof(RadzenStack.Orientation), Orientation.Horizontal);
                            builder.AddAttribute(2, nameof(RadzenStack.Gap), "0");
                            builder.AddAttribute(3, nameof(RadzenStack.ChildContent), (RenderFragment)(childBuilder =>
                            {
                                AddIcon(childBuilder, 4);

                                baseTemplate(item)(childBuilder);
                            }));
                            builder.CloseComponent();
                            break;

                        default: baseTemplate(item)(builder); break;
                    }
                }
            };
        }
    }
}
