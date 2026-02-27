/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public enum ClusterAccessType
{
    Credential = 0,
    [Display(Name = "API Token")] ApiToken = 1
}
