/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Localization;

internal class JsonLocalizerFactory(JsonLocalizationService localizer) : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource) => localizer;
    public IStringLocalizer Create(string baseName, string location) => localizer;
}
