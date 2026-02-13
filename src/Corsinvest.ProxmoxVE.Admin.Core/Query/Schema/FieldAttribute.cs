/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Attribute for defining field metadata
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FieldAttribute : Attribute
{
    public int DefaultFieldSelection { get; set; }
    public object[]? AllowedValues { get; set; }
    public string? DataFormatString { get; set; }
    public string? Description { get; set; }
}
