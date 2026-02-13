/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;

public record Provider(string Name,
                       RenderComponentInfo Render,
                       RenderComponentInfo Settings,
                       string Icon);
