/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "dashboard");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "Dashboards",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                AllowClustersSelect = table.Column<bool>(type: "boolean", nullable: false),
                RefreshInterval = table.Column<int>(type: "integer", nullable: false)
            },
            schema: "dashboard",
            constraints: table => table.PrimaryKey("PK_Dashboards", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Widgets",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                DashboardId = table.Column<int>(type: "integer", nullable: false),
                TitleCss = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                BodyCss = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                X = table.Column<int>(type: "integer", nullable: false),
                Y = table.Column<int>(type: "integer", nullable: false),
                Width = table.Column<int>(type: "integer", nullable: false),
                Height = table.Column<int>(type: "integer", nullable: false),
                SettingsJson = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                ModuleType = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ModuleWidgetType = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "dashboard",
            constraints: table =>
            {
                table.PrimaryKey("PK_Widgets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Widgets_Dashboards_DashboardId",
                    column: x => x.DashboardId,
                    principalTable: "Dashboards",
                    principalColumn: "Id",
                    principalSchema: "dashboard",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Widgets_DashboardId",
            table: "Widgets",
            column: "DashboardId",
            schema: "dashboard");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Widgets",
            schema: "dashboard");

        migrationBuilder.DropTable(
            name: "Dashboards",
            schema: "dashboard");
    }
}
