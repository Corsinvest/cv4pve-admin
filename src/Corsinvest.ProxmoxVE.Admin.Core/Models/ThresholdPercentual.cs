/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public class ThresholdPercentual
{
    [Range(0, 100)]
    public double Warning { get; set; } = 70;

    [Range(0, 100)]
    public double Critical { get; set; } = 80;
}

///// <summary>
///// Settings Threshold
///// </summary>
//public class Threshold<T>
//{
//    /// <summary>
//    /// Warning
//    /// </summary>
//    [DisplayName("Warning")]
//    public T Warning { get; set; }

//    /// <summary>
//    /// Critical
//    /// </summary>
//    /// <value></value>
//    [DisplayName("Critical")]
//    public T Critical { get; set; }
//}
