/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface ISettingsService
{
    object Get(Type settingsType, string section, string key, bool forCurrentUser);
    Task SetAsync(string section, string key, object value, bool forCurrentUser);
    Task SetAsync<TSetting>(TSetting value, bool forCurrentUser = false);
    TSetting Get<TSetting>(bool forCurrentUser = false);
    TSetting Get<TSetting>(string section, string key, bool forCurrentUser);
    //   void Clear(string section, string key, bool forCurrentUser = false);

    ValueTask ClearCacheAsync();
}
