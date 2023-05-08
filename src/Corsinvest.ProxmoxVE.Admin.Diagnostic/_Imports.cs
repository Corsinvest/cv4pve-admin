/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
global using Corsinvest.AppHero.Core.BackgroundJob;
global using Corsinvest.AppHero.Core.BaseUI.DataManager;
global using Corsinvest.AppHero.Core.Domain.Repository;
global using Corsinvest.AppHero.Core.Extensions;
global using Corsinvest.AppHero.Core.Modularity;
global using Corsinvest.AppHero.Core.Options;
global using Corsinvest.AppHero.Core.UI;
global using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
global using Corsinvest.ProxmoxVE.Admin.Core.Repository;
global using Corsinvest.ProxmoxVE.Admin.Diagnostic.Models;
global using Corsinvest.ProxmoxVE.Admin.Diagnostic.Persistence;
global using Microsoft.AspNetCore.Components;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Options;
global using MudBlazor;
global using System.ComponentModel.DataAnnotations;
global using System.Text.RegularExpressions;
