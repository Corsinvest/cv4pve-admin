/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public class ResourceStatusColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Template = item => builder =>
        {
            if (item != null)
            {
                var resource = (item as ClusterResource)!;
                builder.OpenComponent<IconStatusResource>(0);
                builder.AddAttribute(1, nameof(IconStatusResource.Status), resource.Status);
                builder.AddAttribute(2, nameof(IconStatusResource.Type), resource.Type);
                builder.AddAttribute(3, nameof(IconStatusResource.Locked), resource.IsLocked);
                builder.CloseComponent();
            }
        };

        TextAlign = TextAlign.Center;
    }
}
