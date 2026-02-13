/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "system");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                BuiltIn = table.Column<bool>(type: "boolean", nullable: false),
                Default = table.Column<bool>(type: "boolean", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                DisplayName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ProfileImageUrl = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                BuiltIn = table.Column<bool>(type: "boolean", nullable: false),
                UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, collation: "case_insensitive"),
                EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                SecurityStamp = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                ConcurrencyStamp = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                PhoneNumber = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
            },
            schema: "system",
            constraints: table => table.PrimaryKey("PK_AspNetUsers", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Context = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Section = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Key = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Value = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table => table.PrimaryKey("PK_Settings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ClaimType = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                ClaimValue = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                PermissionKey = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Path = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Propagated = table.Column<bool>(type: "boolean", nullable: false),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                BuiltIn = table.Column<bool>(type: "boolean", nullable: false)
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_RolePermissions_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ClaimType = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                ClaimValue = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ProviderKey = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                ProviderDisplayName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                RoleId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                LoginProvider = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Value = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserPermissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                PermissionKey = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Path = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                Propagated = table.Column<bool>(type: "boolean", nullable: false),
                ClusterName = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                BuiltIn = table.Column<bool>(type: "boolean", nullable: false)
            },
            schema: "system",
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserPermissions_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    principalSchema: "system",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            schema: "system",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail",
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            schema: "system",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionKey_Path_ClusterName",
            table: "RolePermissions",
            columns: ["RoleId", "PermissionKey", "Path", "ClusterName"],
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_Settings_Context_Section_Key",
            table: "Settings",
            columns: ["Context", "Section", "Key"],
            schema: "system");

        migrationBuilder.CreateIndex(
            name: "IX_UserPermissions_UserId_PermissionKey_Path_ClusterName",
            table: "UserPermissions",
            columns: ["UserId", "PermissionKey", "Path", "ClusterName"],
            schema: "system");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens",
            schema: "system");

        migrationBuilder.DropTable(
            name: "RolePermissions",
            schema: "system");

        migrationBuilder.DropTable(
            name: "Settings",
            schema: "system");

        migrationBuilder.DropTable(
            name: "UserPermissions",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetRoles",
            schema: "system");

        migrationBuilder.DropTable(
            name: "AspNetUsers",
            schema: "system");
    }
}
