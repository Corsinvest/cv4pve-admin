/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Migrations;

/// <inheritdoc />
public partial class AddAuditLog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                UserName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Action = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Success = table.Column<bool>(type: "boolean", nullable: false),
                Details = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                IpAddress = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                UserAgent = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_AuditLogs_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Action",
            table: "AuditLogs",
            column: "Action",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Success",
            table: "AuditLogs",
            column: "Success",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Timestamp",
            table: "AuditLogs",
            column: "Timestamp",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId",
            table: "AuditLogs",
            column: "UserId",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserName_Timestamp",
            table: "AuditLogs",
            columns: ["UserName", "Timestamp"],
            schema: "system");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "AuditLogs", schema: "system");
}
