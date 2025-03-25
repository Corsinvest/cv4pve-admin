/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Auditing.Persistence.Context;
using Corsinvest.AppHero.Auditing.Persistence.Interceptors;
using Corsinvest.AppHero.Core.Security.Identity;
using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
                                  AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor) : BaseApplicationDbContext<ApplicationUser, ApplicationRole>(options, auditableEntitySaveChangesInterceptor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseCollation("NOCASE");
    }
}
