using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Migrations;

/// <inheritdoc />
public partial class AddReportFormat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Format",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Format",
            schema: "system_reports",
            table: "JobResults");
    }
}
