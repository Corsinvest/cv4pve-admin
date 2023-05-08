/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Persistence;

internal class ClusterUsageDbContext : DbContext
{
    public ClusterUsageDbContext(DbContextOptions<ClusterUsageDbContext> options) : base(options) { }

    public DbSet<DataVm> DataVms { get; set; } = default!;
    public DbSet<DataVmStorage> DataVmStorages { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseCollation("NOCASE");

        modelBuilder.Entity<DataVm>()
                    .HasMany(c => c.Storages)
                    .WithOne(a => a.DataVm)
                    .OnDelete(DeleteBehavior.Cascade);
    }
}