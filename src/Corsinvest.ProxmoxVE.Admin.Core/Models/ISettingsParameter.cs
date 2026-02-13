/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface ISettingsParameter<T>
{
    [Parameter] T Settings { get; set; }
    [Parameter] EventCallback<T> SettingsChanged { get; set; }
}
