/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "diagnostic");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "IgnoredIssues",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                IdResource = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Gravity = table.Column<int>(type: "integer", nullable: false),
                Context = table.Column<int>(type: "integer", nullable: false),
                SubContext = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                Description = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "diagnostic",
            constraints: table => table.PrimaryKey("PK_IgnoredIssues", x => x.Id));

        migrationBuilder.CreateTable(
            name: "JobResults",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Warning = table.Column<int>(type: "integer", nullable: false),
                Critical = table.Column<int>(type: "integer", nullable: false),
                Info = table.Column<int>(type: "integer", nullable: false),
                Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            schema: "diagnostic",
            constraints: table => table.PrimaryKey("PK_JobResults", x => x.Id));

        migrationBuilder.CreateTable(
            name: "JobDetails",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                JobResultId = table.Column<int>(type: "integer", nullable: false),
                IdResource = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Context = table.Column<int>(type: "integer", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Gravity = table.Column<int>(type: "integer", nullable: false),
                IsIgnoredIssue = table.Column<bool>(type: "boolean", nullable: false),
                SubContext = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "diagnostic",
            constraints: table =>
            {
                table.PrimaryKey("PK_JobDetails", x => x.Id);
                table.ForeignKey(
                    name: "FK_JobDetails_JobResults_JobResultId",
                    column: x => x.JobResultId,
                    principalTable: "JobResults",
                    principalColumn: "Id",
                    principalSchema: "diagnostic",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_IgnoredIssues_ClusterName",
            table: "IgnoredIssues",
            column: "ClusterName",
            schema: "diagnostic");

        migrationBuilder.CreateIndex(
            name: "IX_JobDetails_JobResultId",
            table: "JobDetails",
            column: "JobResultId",
            schema: "diagnostic");

        migrationBuilder.CreateIndex(
            name: "IX_JobResults_ClusterName",
            table: "JobResults",
            column: "ClusterName",
            schema: "diagnostic");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "IgnoredIssues",
            schema: "diagnostic");

        migrationBuilder.DropTable(
            name: "JobDetails",
            schema: "diagnostic");

        migrationBuilder.DropTable(
            name: "JobResults",
            schema: "diagnostic");
    }
}
