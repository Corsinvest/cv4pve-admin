/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.ClusterUsage.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataVms",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ClusterName = table.Column<string>(type: "TEXT", nullable: false),
                VmId = table.Column<long>(type: "INTEGER", nullable: false),
                VmName = table.Column<string>(type: "TEXT", nullable: false),
                Node = table.Column<string>(type: "TEXT", nullable: false),
                CpuSize = table.Column<int>(type: "INTEGER", nullable: false),
                CpuUsagePercentage = table.Column<double>(type: "REAL", nullable: false),
                MemorySize = table.Column<long>(type: "INTEGER", nullable: false),
                MemoryUsage = table.Column<long>(type: "INTEGER", nullable: false),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataVms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "DataVmStorages",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                DataVmId = table.Column<int>(type: "INTEGER", nullable: false),
                Storage = table.Column<string>(type: "TEXT", nullable: false),
                Size = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataVmStorages", x => x.Id);
                table.ForeignKey(
                    name: "FK_DataVmStorages_DataVms_DataVmId",
                    column: x => x.DataVmId,
                    principalTable: "DataVms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DataVmStorages_DataVmId",
            table: "DataVmStorages",
            column: "DataVmId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DataVmStorages");

        migrationBuilder.DropTable(
            name: "DataVms");
    }
}
