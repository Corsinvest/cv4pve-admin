using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Migrations;

/// <inheritdoc />
public partial class AlignSnapshot : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SettingsJson",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.AddColumn<string>(
            name: "Settings",
            schema: "system_reports",
            table: "JobResults",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Settings",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.AddColumn<string>(
            name: "SettingsJson",
            schema: "system_reports",
            table: "JobResults",
            type: "text",
            nullable: false,
            defaultValue: "",
            collation: "case_insensitive");
    }
}
