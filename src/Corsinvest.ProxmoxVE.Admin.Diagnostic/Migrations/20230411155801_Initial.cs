/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Executions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClusterName = table.Column<string>(type: "TEXT", nullable: false),
                    Warning = table.Column<int>(type: "INTEGER", nullable: false),
                    Critical = table.Column<int>(type: "INTEGER", nullable: false),
                    Info = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IgnoredIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClusterName = table.Column<string>(type: "TEXT", nullable: false),
                    IdResource = table.Column<string>(type: "TEXT", nullable: true),
                    Gravity = table.Column<int>(type: "INTEGER", nullable: false),
                    Context = table.Column<int>(type: "INTEGER", nullable: false),
                    SubContext = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoredIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExecutionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutionDatas_Executions_ExecutionId",
                        column: x => x.ExecutionId,
                        principalTable: "Executions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionDatas_ExecutionId",
                table: "ExecutionDatas",
                column: "ExecutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionDatas");

            migrationBuilder.DropTable(
                name: "IgnoredIssues");

            migrationBuilder.DropTable(
                name: "Executions");
        }
    }
}
