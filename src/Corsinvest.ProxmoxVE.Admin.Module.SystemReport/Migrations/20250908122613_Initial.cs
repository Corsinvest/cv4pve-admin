using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "system_reports");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "JobResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                NodeNames = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                NodeFeatures = table.Column<int>(type: "integer", nullable: false),
                VmIds = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                VmFeatures = table.Column<int>(type: "integer", nullable: false),
                StorageNames = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                StorageFeatures = table.Column<int>(type: "integer", nullable: false),
                RrdDataTimeFrame = table.Column<int>(type: "integer", nullable: false),
                RrdDataConsolidation = table.Column<int>(type: "integer", nullable: false),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Status = table.Column<bool>(type: "boolean", nullable: false),
                Logs = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "system_reports",
            constraints: table => table.PrimaryKey("PK_JobResults", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_ClusterName",
            table: "JobResults",
            column: "ClusterName",
            schema: "system_reports");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_End",
            table: "JobResults",
            column: "End",
            schema: "system_reports");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_Start",
            table: "JobResults",
            column: "Start",
            schema: "system_reports");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
            name: "JobResults",
            schema: "system_reports");
}
