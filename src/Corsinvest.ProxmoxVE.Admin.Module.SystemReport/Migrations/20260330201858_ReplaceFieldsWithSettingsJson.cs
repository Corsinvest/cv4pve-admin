using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Migrations;

/// <inheritdoc />
public partial class ReplaceFieldsWithSettingsJson : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NodeFeatures",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "NodeNames",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "RrdDataConsolidation",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "RrdDataTimeFrame",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "StorageFeatures",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "StorageNames",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.DropColumn(
            name: "VmFeatures",
            schema: "system_reports",
            table: "JobResults");

        migrationBuilder.RenameColumn(
            name: "VmIds",
            schema: "system_reports",
            table: "JobResults",
            newName: "SettingsJson");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "SettingsJson",
            schema: "system_reports",
            table: "JobResults",
            newName: "VmIds");

        migrationBuilder.AddColumn<int>(
            name: "NodeFeatures",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "NodeNames",
            schema: "system_reports",
            table: "JobResults",
            type: "text",
            nullable: false,
            defaultValue: "",
            collation: "case_insensitive");

        migrationBuilder.AddColumn<int>(
            name: "RrdDataConsolidation",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "RrdDataTimeFrame",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "StorageFeatures",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "StorageNames",
            schema: "system_reports",
            table: "JobResults",
            type: "text",
            nullable: false,
            defaultValue: "",
            collation: "case_insensitive");

        migrationBuilder.AddColumn<int>(
            name: "VmFeatures",
            schema: "system_reports",
            table: "JobResults",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
