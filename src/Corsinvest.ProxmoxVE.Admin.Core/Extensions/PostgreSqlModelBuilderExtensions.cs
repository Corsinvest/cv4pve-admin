using Microsoft.EntityFrameworkCore;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class PostgreSqlModelBuilderExtensions
{
    /// <summary>
    /// Applies PostgreSQL ICU case-insensitive collation to all string properties in the model
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance</param>
    /// <param name="collationName">Name of the collation (default: "case_insensitive")</param>
    public static void ApplyPostgreSqlCaseInsensitiveCollation(this ModelBuilder modelBuilder, string collationName = "case_insensitive")
    {
        // Create PostgreSQL ICU collation
        modelBuilder.HasCollation(collationName,
                                  locale: "en-u-ks-primary",
                                  provider: "icu",
                                  deterministic: false);

        // Apply collation to all string properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string)) { property.SetCollation(collationName); }
            }
        }
    }

    /// <summary>
    /// Applies PostgreSQL ICU case and accent insensitive collation to all string properties
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance</param>
    /// <param name="collationName">Name of the collation (default: "case_accent_insensitive")</param>
    public static void ApplyPostgreSqlCaseAccentInsensitiveCollation(this ModelBuilder modelBuilder, string collationName = "case_accent_insensitive")
    {
        // Create PostgreSQL ICU collation (more permissive)
        modelBuilder.HasCollation(collationName,
                                  locale: "und-u-ks-level1",
                                  provider: "icu",
                                  deterministic: false);

        // Apply collation to all string properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string)) { property.SetCollation(collationName); }
            }
        }
    }
}
