/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Support.Subscription;

public enum Status
{
    /// subscription set and active
    Active,
    /// subscription set but invalid for this server
    Invalid,
    /// subscription set but expired for this server
    Expired,
    /// subscription got (recently) suspended
    Suspended,
}