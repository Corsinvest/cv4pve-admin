using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Migrations;

/// <inheritdoc />
public partial class UserSessions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserSessions",
            schema: "system",
            columns: table => new
            {
                Key = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                UserId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Ticket = table.Column<byte[]>(type: "bytea", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSessions", x => x.Key);
                table.ForeignKey(
                    name: "FK_UserSessions_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "system",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_ExpiresAt",
            schema: "system",
            table: "UserSessions",
            column: "ExpiresAt");

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_UserId",
            schema: "system",
            table: "UserSessions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserSessions",
            schema: "system");
    }
}
