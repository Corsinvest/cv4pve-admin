/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.NodeProtect.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "node_protect");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "FolderTaskResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TaskId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Node = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Size = table.Column<long>(type: "bigint", nullable: false),
                Status = table.Column<bool>(type: "boolean", nullable: false),
                FileName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Logs = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "node_protect",
            constraints: table => table.PrimaryKey("PK_FolderTaskResults", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_FolderTaskResults_ClusterName",
            table: "FolderTaskResults",
            column: "ClusterName",
            schema: "node_protect");

        migrationBuilder.CreateIndex(
            name: "IX_FolderTaskResults_End",
            table: "FolderTaskResults",
            column: "End",
            schema: "node_protect");

        migrationBuilder.CreateIndex(
            name: "IX_FolderTaskResults_Start",
            table: "FolderTaskResults",
            column: "Start",
            schema: "node_protect");

        migrationBuilder.CreateIndex(
            name: "IX_FolderTaskResults_TaskId",
            table: "FolderTaskResults",
            column: "TaskId",
            schema: "node_protect");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
            name: "FolderTaskResults",
            schema: "node_protect");
}
