/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Persistence;

public class AutoSnapDbContext : DbContext
{
    public AutoSnapDbContext(DbContextOptions<AutoSnapDbContext> options) : base(options) { }
    public DbSet<AutoSnapJob> Jobs { get; set; } = default!;
    public DbSet<AutoSnapJobHistory> JobHistories { get; set; } = default!;
    public DbSet<AutoSnapJobHook> JobHooks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseCollation("NOCASE");

        modelBuilder.Entity<AutoSnapJob>()
                    .HasMany(c => c.Histories)
                    .WithOne(a => a.Job)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AutoSnapJob>()
                    .HasMany(c => c.Hooks)
                    .WithOne(a => a.Job)
                    .OnDelete(DeleteBehavior.Cascade);
    }
}