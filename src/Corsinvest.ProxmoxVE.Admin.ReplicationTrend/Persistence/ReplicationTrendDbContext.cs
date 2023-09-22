/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Persistence;

public class ReplicationTrendDbContext : DbContext
{
    public ReplicationTrendDbContext(DbContextOptions<ReplicationTrendDbContext> options) : base(options) { }

    public DbSet<ReplicationResult> ReplicationResults { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("NOCASE");

        modelBuilder.Entity<ReplicationResult>().HasIndex(a => a.LastSync);
        modelBuilder.Entity<ReplicationResult>().HasIndex(a => a.Start);
        modelBuilder.Entity<ReplicationResult>().HasIndex(a => a.End);
        modelBuilder.Entity<ReplicationResult>().HasIndex(a => a.VmId);
    }
}