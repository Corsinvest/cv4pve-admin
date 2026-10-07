using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Migrations
{
    /// <inheritdoc />
    public partial class AddIgnoredIssueErrorCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                schema: "diagnostic",
                table: "IgnoredIssues",
                type: "text",
                nullable: true,
                collation: "case_insensitive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorCode",
                schema: "diagnostic",
                table: "IgnoredIssues");
        }
    }
}
