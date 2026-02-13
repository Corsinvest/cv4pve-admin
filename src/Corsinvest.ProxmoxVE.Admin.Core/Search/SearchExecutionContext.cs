/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Search;

public record SearchExecutionContext(SearchResultItem Item,
                                     string ClusterName,
                                     NavigationManager NavigationManager);
