using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.Dashboard.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWidgetModuleType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuleType",
                schema: "dashboard",
                table: "Widgets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModuleType",
                schema: "dashboard",
                table: "Widgets",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive");
        }
    }
}
