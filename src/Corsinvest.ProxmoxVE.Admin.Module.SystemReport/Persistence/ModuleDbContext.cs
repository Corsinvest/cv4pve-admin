/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;

public class ModuleDbContext(DbContextOptions<ModuleDbContext> options) : ModuleDbContextBase<ModuleDbContext>(options)
{
    public DbSet<JobResult> JobResults { get; set; } = default!;

    protected override string SchemaName => "system_reports";

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobResult>().HasIndex(a => a.ClusterName);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<JobResult>().HasIndex(a => a.End);
    }
}
