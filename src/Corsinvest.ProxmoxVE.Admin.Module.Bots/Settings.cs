/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations;
using Corsinvest.ProxmoxVE.Admin.Core.Modularity;

namespace Corsinvest.ProxmoxVE.Admin.Module.Bots;

public class Settings : IModuleSettings
{
    [Required] public string ClusterName { get; set; } = default!;
    public bool Enabled { get; set; }

    public TelegramSettings Telegram { get; set; } = new();

    public class TelegramSettings : IEnabled
    {
        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Required]
        [Display(Name = "BOT API Token")]
        [Encrypt]
        public string Token { get; set; } = default!;

        [Display(Name = "Chats Id (one per line). Empty non control access.")]
        public string ChatsId { get; set; } = default!;

        // A value saved by the old single-line field has no line break: commas, semicolons and spaces separate too
        private static readonly char[] _chatsIdSeparators = ['\r', '\n', ',', ';', ' '];

        public long[] GetChatsId()
           => [.. (ChatsId ?? string.Empty).Split(_chatsIdSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                           .Select(a => new
                                           {
                                             Valid = long.TryParse(a, out var chatId),
                                             ChatId = chatId
                                           })
                                           .Where(a => a.Valid)
                                           .Select(a => a.ChatId)];
    }
}
