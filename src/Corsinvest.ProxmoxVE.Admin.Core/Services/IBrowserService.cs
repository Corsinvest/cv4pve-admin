/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IBrowserService
{
    Task CopyToClipboardAsync(string text);
    Task OpenAsync(string url, string target);
    Task OpenInNewWindowAsync(string url, string windowFeatures);
}
