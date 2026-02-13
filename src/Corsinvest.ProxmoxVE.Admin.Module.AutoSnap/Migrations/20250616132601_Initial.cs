/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "autosnap");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "Jobs",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Description = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                VmIds = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Label = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Keep = table.Column<int>(type: "integer", nullable: false),
                VmStatus = table.Column<bool>(type: "boolean", nullable: false),
                OnlyRuns = table.Column<bool>(type: "boolean", nullable: false),
                TimeoutSnapshot = table.Column<long>(type: "bigint", nullable: false),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                CronExpression = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "autosnap",
            constraints: table => table.PrimaryKey("PK_Jobs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Results",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                JobId = table.Column<int>(type: "integer", nullable: false),
                SnapName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Status = table.Column<bool>(type: "boolean", nullable: false),
                Logs = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "autosnap",
            constraints: table =>
            {
                table.PrimaryKey("PK_Results", x => x.Id);
                table.ForeignKey(
                    name: "FK_Results_Jobs_JobId",
                    column: x => x.JobId,
                    principalTable: "Jobs",
                    principalColumn: "Id",
                    principalSchema: "autosnap",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "WebHooks",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Description = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                IgnoreSslCertificate = table.Column<bool>(type: "boolean", nullable: false),
                OrderIndex = table.Column<int>(type: "integer", nullable: false),
                Phase = table.Column<int>(type: "integer", nullable: false),
                Url = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Method = table.Column<int>(type: "integer", nullable: false),
                Header = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Body = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                JobScheduleId = table.Column<int>(type: "integer", nullable: true)
            },
            schema: "autosnap",
            constraints: table =>
            {
                table.PrimaryKey("PK_WebHooks", x => x.Id);
                table.ForeignKey(
                    name: "FK_WebHooks_Jobs_JobScheduleId",
                    column: x => x.JobScheduleId,
                    principalTable: "Jobs",
                    principalColumn: "Id",
                    principalSchema: "autosnap");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_ClusterName",
            table: "Jobs",
            column: "ClusterName",
            schema: "autosnap");

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_ClusterName_Label",
            table: "Jobs",
            columns: ["ClusterName", "Label"],
            schema: "autosnap",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_Label",
            table: "Jobs",
            column: "Label",
            schema: "autosnap");

        migrationBuilder.CreateIndex(
            name: "IX_Results_End",
            table: "Results",
            column: "End",
            schema: "autosnap");

        migrationBuilder.CreateIndex(
            name: "IX_Results_JobId",
            table: "Results",
            column: "JobId",
            schema: "autosnap");

        migrationBuilder.CreateIndex(
            name: "IX_Results_Start",
            table: "Results",
            column: "Start",
            schema: "autosnap");

        migrationBuilder.CreateIndex(
            name: "IX_WebHooks_JobScheduleId",
            table: "WebHooks",
            column: "JobScheduleId",
            schema: "autosnap");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Results",
            schema: "autosnap");

        migrationBuilder.DropTable(
            name: "WebHooks",
            schema: "autosnap");

        migrationBuilder.DropTable(
            name: "Jobs",
            schema: "autosnap");
    }
}
