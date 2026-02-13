/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Models;

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<FolderTaskResult> FolderTaskResults { get; set; } = default!;

    protected override string SchemaName => "node_protect";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FolderTaskResult>().HasIndex(a => a.ClusterName);
        modelBuilder.Entity<FolderTaskResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<FolderTaskResult>().HasIndex(a => a.End);
        modelBuilder.Entity<FolderTaskResult>().HasIndex(a => a.TaskId);
    }
}
