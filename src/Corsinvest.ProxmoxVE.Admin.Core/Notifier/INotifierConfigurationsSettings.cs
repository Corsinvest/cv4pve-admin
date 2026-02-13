/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Notifier;

public interface INotifierConfigurationsSettings
{
    [Display(Name = "Notifiers")]
    IEnumerable<string> NotifierConfigurations { get; set; }
}
