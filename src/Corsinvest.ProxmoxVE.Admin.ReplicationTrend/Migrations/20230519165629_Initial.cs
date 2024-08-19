/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.ReplicationTrend.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ReplicationResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ClusterName = table.Column<string>(type: "TEXT", nullable: false),
                JobId = table.Column<string>(type: "TEXT", nullable: false),
                Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                End = table.Column<DateTime>(type: "TEXT", nullable: true),
                Duration = table.Column<double>(type: "REAL", nullable: false),
                VmId = table.Column<string>(type: "TEXT", nullable: false),
                Size = table.Column<double>(type: "REAL", nullable: false),
                Log = table.Column<string>(type: "TEXT", nullable: false),
                LastSync = table.Column<DateTime>(type: "TEXT", nullable: false),
                Error = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReplicationResults", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ReplicationResults_End",
            table: "ReplicationResults",
            column: "End");

        migrationBuilder.CreateIndex(
            name: "IX_ReplicationResults_LastSync",
            table: "ReplicationResults",
            column: "LastSync");

        migrationBuilder.CreateIndex(
            name: "IX_ReplicationResults_Start",
            table: "ReplicationResults",
            column: "Start");

        migrationBuilder.CreateIndex(
            name: "IX_ReplicationResults_VmId",
            table: "ReplicationResults",
            column: "VmId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ReplicationResults");
    }
}
