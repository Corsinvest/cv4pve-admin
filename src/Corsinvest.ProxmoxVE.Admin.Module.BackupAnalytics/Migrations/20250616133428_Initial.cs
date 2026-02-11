using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.BackupAnalytics.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "backup_insights");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "TaskResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TaskId = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Status = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Node = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Logs = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Storage = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "backup_insights",
            constraints: table => table.PrimaryKey("PK_TaskResults", x => x.Id));

        migrationBuilder.CreateTable(
            name: "JobResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TaskResultId = table.Column<int>(type: "integer", nullable: false),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                VmId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Size = table.Column<double>(type: "double precision", nullable: false),
                Error = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Status = table.Column<bool>(type: "boolean", nullable: false),
                TransferSize = table.Column<double>(type: "double precision", nullable: false),
                Archive = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Logs = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "backup_insights",
            constraints: table =>
            {
                table.PrimaryKey("PK_JobResults", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobResults_TaskResults_TaskResultId",
                    column: x => x.TaskResultId,
                    principalTable: "TaskResults",
                    principalColumn: "Id",
                    principalSchema: "backup_insights",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_End",
            table: "JobResults",
            column: "End",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_Start",
            table: "JobResults",
            column: "Start",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_TaskResultId",
            table: "JobResults",
            column: "TaskResultId",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_VmId",
            table: "JobResults",
            column: "VmId",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_TaskResults_ClusterName",
            table: "TaskResults",
            column: "ClusterName",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_TaskResults_End",
            table: "TaskResults",
            column: "End",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_TaskResults_Start",
            table: "TaskResults",
            column: "Start",
            schema: "backup_insights");

        migrationBuilder.CreateIndex(
            name: "IX_TaskResults_TaskId",
            table: "TaskResults",
            column: "TaskId",
            schema: "backup_insights");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "JobResults",
            schema: "backup_insights");

        migrationBuilder.DropTable(
            name: "TaskResults",
            schema: "backup_insights");
    }
}
