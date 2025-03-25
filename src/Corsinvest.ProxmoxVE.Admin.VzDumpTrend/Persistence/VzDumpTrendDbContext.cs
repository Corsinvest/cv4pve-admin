/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Persistence;

public class VzDumpTrendDbContext(DbContextOptions<VzDumpTrendDbContext> options) : DbContext(options)
{
    public DbSet<VzDumpTask> VzDumpTasks { get; set; } = default!;
    public DbSet<VzDumpDetail> VzDumpDetails { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("NOCASE");

        modelBuilder.Entity<VzDumpTask>()
                    .HasMany(c => c.Details)
                    .WithOne(a => a.Task)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VzDumpTask>().HasIndex(a => a.Storage);
        modelBuilder.Entity<VzDumpTask>().HasIndex(a => a.Start);
        modelBuilder.Entity<VzDumpDetail>().HasIndex(a => a.End);
        modelBuilder.Entity<VzDumpDetail>().HasIndex(a => a.VmId);
    }
}