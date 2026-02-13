/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<TaskResult> TaskResults { get; set; } = default!;
    public DbSet<JobResult> JobResults { get; set; } = default!;

    protected override string SchemaName => "backup_insights";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskResult>().HasIndex(a => a.ClusterName);
        modelBuilder.Entity<TaskResult>().HasIndex(a => a.TaskId);
        modelBuilder.Entity<TaskResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<TaskResult>().HasIndex(a => a.End);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.End);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.VmId);
    }
}
