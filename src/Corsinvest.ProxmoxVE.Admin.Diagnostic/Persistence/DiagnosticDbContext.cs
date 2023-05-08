/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Persistence;

public class DiagnosticDbContext : DbContext
{
    public DiagnosticDbContext(DbContextOptions<DiagnosticDbContext> options) : base(options) { }
    public DbSet<Execution> Executions { get; set; } = default!;
    public DbSet<ExecutionData> ExecutionDatas { get; set; } = default!;
    public DbSet<IgnoredIssue> IgnoredIssues { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseCollation("NOCASE");

        modelBuilder.Entity<Execution>()
                    .HasOne(a => a.Data)
                    .WithOne(a => a.Execution)
                    .HasForeignKey<ExecutionData>(a => a.ExecutionId)
                    .OnDelete(DeleteBehavior.Cascade);
    }
}