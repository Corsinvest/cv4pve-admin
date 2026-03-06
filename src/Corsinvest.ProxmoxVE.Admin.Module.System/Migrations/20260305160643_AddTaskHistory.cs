using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskItems",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    ModuleName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    ReferenceId = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    DetailUrl = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    IsCancellable = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeoutAfter = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LastLog = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    Logs = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_StartedAt",
                schema: "system",
                table: "TaskItems",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_Status",
                schema: "system",
                table: "TaskItems",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskItems",
                schema: "system");
        }
    }
}
