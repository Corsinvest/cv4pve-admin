using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Migrations
{
    /// <inheritdoc />
    public partial class AddJobDetailCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                schema: "diagnostic",
                table: "JobDetails",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive");

            migrationBuilder.CreateTable(
                name: "JobDetailCompliances",
                schema: "diagnostic",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobDetailId = table.Column<int>(type: "integer", nullable: false),
                    Standard = table.Column<int>(type: "integer", nullable: false),
                    ControlId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDetailCompliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobDetailCompliances_JobDetails_JobDetailId",
                        column: x => x.JobDetailId,
                        principalSchema: "diagnostic",
                        principalTable: "JobDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobDetailCompliances_JobDetailId",
                schema: "diagnostic",
                table: "JobDetailCompliances",
                column: "JobDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDetailCompliances_Standard",
                schema: "diagnostic",
                table: "JobDetailCompliances",
                column: "Standard");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobDetailCompliances",
                schema: "diagnostic");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                schema: "diagnostic",
                table: "JobDetails");
        }
    }
}
