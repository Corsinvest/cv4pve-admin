/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;
using System.ComponentModel.DataAnnotations;

namespace Corsinvest.ProxmoxVE.Admin.Botgram;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName
{
    public string ClusterName { get; set; } = default!;

    [Display(Name = "Enabled")]
    public bool Enabled { get; set; }

    [Display(Name = "BOT API Token")]
    public string Token { get; set; } = default!;

    [Display(Name = "Chats Id (new line separator). Empty non control access.")]
    public string ChatsId { get; set; } = default!;

    public long[] GetChatsId()
        => [.. ChatsId.Split(Environment.NewLine)
                      .Select(a => new
                      {
                        Valid = long.TryParse(a, out var chatId),
                        ChatId = chatId
                      })
                      .Where(a => a.Valid)
                      .Select(a => a.ChatId)];
}
