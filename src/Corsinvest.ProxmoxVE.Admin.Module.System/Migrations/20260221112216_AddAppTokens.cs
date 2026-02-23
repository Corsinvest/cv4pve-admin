using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Migrations;

/// <inheritdoc />
public partial class AddAppTokens : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppTokens",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                TokenHash = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                OwnerId = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppTokens_AspNetUsers_OwnerId",
                    column: x => x.OwnerId,
                    principalSchema: "system",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "AppTokenPermissions",
            schema: "system",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                AppTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                PermissionKey = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Path = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Propagated = table.Column<bool>(type: "boolean", nullable: false),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                BuiltIn = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppTokenPermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppTokenPermissions_AppTokens_AppTokenId",
                    column: x => x.AppTokenId,
                    principalSchema: "system",
                    principalTable: "AppTokens",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AppTokenRoles",
            schema: "system",
            columns: table => new
            {
                AppTokenId = table.Column<Guid>(type: "uuid", nullable: false),
                RoleId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppTokenRoles", x => new { x.AppTokenId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AppTokenRoles_AppTokens_AppTokenId",
                    column: x => x.AppTokenId,
                    principalSchema: "system",
                    principalTable: "AppTokens",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AppTokenRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "system",
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppTokenPermissions_AppTokenId_PermissionKey_Path_ClusterNa~",
            schema: "system",
            table: "AppTokenPermissions",
            columns: new[] { "AppTokenId", "PermissionKey", "Path", "ClusterName" });

        migrationBuilder.CreateIndex(
            name: "IX_AppTokenRoles_RoleId",
            schema: "system",
            table: "AppTokenRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_AppTokens_Name",
            schema: "system",
            table: "AppTokens",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_AppTokens_OwnerId",
            schema: "system",
            table: "AppTokens",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_AppTokens_TokenHash",
            schema: "system",
            table: "AppTokens",
            column: "TokenHash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppTokenPermissions",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AppTokenRoles",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AppTokens",
            schema: "system");
    }
}
