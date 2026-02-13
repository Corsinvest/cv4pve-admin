/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.ComponentModel.DataAnnotations.Schema;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Schema;

/// <summary>
/// Storage content information (includes backups)
/// </summary>
[Table("backups")]
[Description("Storage Content")]
public class StorageContentInfo
{
    public long GuestId { get; set; }

    public string NodeName { get; set; } = default!;

    public string StorageName { get; set; } = default!;

    [Field(DataFormatString = FormatHelper.FormatBytes)]
    public long SizeBytes { get; set; }

    public string FileName { get; set; } = default!;

    [Field(AllowedValues = ["images", "backup", "vztmpl", "iso", "rootdir", "snippets"])]
    public string Content { get; set; } = default!;

    public DateTime CreationDate { get; set; }

    public string Format { get; set; } = default!;

    public bool BackupEncrypted { get; set; }

    public bool BackupVerified { get; set; }

    public bool BackupProtected { get; set; }

    public string Name { get; set; } = default!;

    public string Parent { get; set; } = default!;

    public string Notes { get; set; } = default!;

    public string ContentDescription { get; set; } = default!;

    public static StorageContentInfo Map(NodeStorageContent content, string nodeName, string storageName)
        => new()
        {
            NodeName = nodeName,
            StorageName = storageName,
            SizeBytes = content.Size,
            FileName = content.FileName,
            Content = content.Content,
            CreationDate = content.CreationDate,
            Format = content.Format,
            BackupEncrypted = content.Encrypted,
            BackupVerified = content.Verified,
            BackupProtected = content.Protected,
            Name = content.Name,
            Parent = content.Parent,
            Notes = content.Notes,
            ContentDescription = content.ContentDescription,
            GuestId = content.VmId
        };
}
