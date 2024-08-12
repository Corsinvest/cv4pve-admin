/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.VzDumpTrend.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VzDumpTasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ClusterName = table.Column<string>(type: "TEXT", nullable: false),
                Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                End = table.Column<DateTime>(type: "TEXT", nullable: true),
                TaskId = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", nullable: true),
                Node = table.Column<string>(type: "TEXT", nullable: true),
                Log = table.Column<string>(type: "TEXT", nullable: true),
                Storage = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VzDumpTasks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "VzDumpDetails",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                Start = table.Column<DateTime>(type: "TEXT", nullable: true),
                End = table.Column<DateTime>(type: "TEXT", nullable: true),
                VmId = table.Column<string>(type: "TEXT", nullable: true),
                Size = table.Column<double>(type: "REAL", nullable: false),
                Error = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<bool>(type: "INTEGER", nullable: false),
                TransferSize = table.Column<double>(type: "REAL", nullable: false),
                Archive = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VzDumpDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_VzDumpDetails_VzDumpTasks_TaskId",
                    column: x => x.TaskId,
                    principalTable: "VzDumpTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VzDumpDetails_End",
            table: "VzDumpDetails",
            column: "End");

        migrationBuilder.CreateIndex(
            name: "IX_VzDumpDetails_TaskId",
            table: "VzDumpDetails",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_VzDumpDetails_VmId",
            table: "VzDumpDetails",
            column: "VmId");

        migrationBuilder.CreateIndex(
            name: "IX_VzDumpTasks_Start",
            table: "VzDumpTasks",
            column: "Start");

        migrationBuilder.CreateIndex(
            name: "IX_VzDumpTasks_Storage",
            table: "VzDumpTasks",
            column: "Storage");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "VzDumpDetails");

        migrationBuilder.DropTable(
            name: "VzDumpTasks");
    }
}
