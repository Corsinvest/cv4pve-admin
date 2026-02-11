using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.ReplicationAnalytics.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "replication_insights");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "JobResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                JobId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                VmId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Size = table.Column<double>(type: "double precision", nullable: false),
                Logs = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Error = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Status = table.Column<bool>(type: "boolean", nullable: false),
                Source = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Target = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "replication_insights",
            constraints: table => table.PrimaryKey("PK_JobResults", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_ClusterName",
            table: "JobResults",
            column: "ClusterName",
            schema: "replication_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_End",
            table: "JobResults",
            column: "End",
            schema: "replication_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_LastSync",
            table: "JobResults",
            column: "LastSync",
            schema: "replication_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_Start",
            table: "JobResults",
            column: "Start",
            schema: "replication_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_VmId",
            table: "JobResults",
            column: "VmId",
            schema: "replication_insights");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
            name: "JobResults",
            schema: "replication_insights");
}
