/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

public enum Notify
{
    None = 0,
    Allways = 1,

    [Display(Description = "On Failure Only")]
    OnFailureOnly = 2
}
