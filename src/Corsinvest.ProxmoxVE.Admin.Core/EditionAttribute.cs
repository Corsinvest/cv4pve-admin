/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class EditionAttribute(string edition) : Attribute
{
    public string Edition { get; } = edition;
}
