using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Migrations;

/// <inheritdoc />
public partial class AddExtendedDataRemoveWebHooks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WebHooks",
            schema: "autosnap");

        migrationBuilder.AddColumn<string>(
            name: "ExtendedDataJson",
            schema: "autosnap",
            table: "Jobs",
            type: "text",
            nullable: false,
            defaultValue: "",
            collation: "case_insensitive");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ExtendedDataJson",
            schema: "autosnap",
            table: "Jobs");

        migrationBuilder.CreateTable(
            name: "WebHooks",
            schema: "autosnap",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Body = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Description = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                Header = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                IgnoreSslCertificate = table.Column<bool>(type: "boolean", nullable: false),
                JobScheduleId = table.Column<int>(type: "integer", nullable: true),
                Method = table.Column<int>(type: "integer", nullable: false),
                OrderIndex = table.Column<int>(type: "integer", nullable: false),
                Phase = table.Column<int>(type: "integer", nullable: false),
                Url = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebHooks", x => x.Id);
                table.ForeignKey(
                    name: "FK_WebHooks_Jobs_JobScheduleId",
                    column: x => x.JobScheduleId,
                    principalSchema: "autosnap",
                    principalTable: "Jobs",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_WebHooks_JobScheduleId",
            schema: "autosnap",
            table: "WebHooks",
            column: "JobScheduleId");
    }
}
