using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddRemainingAmountColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "OrganizationSettlement",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "OrganizationSettlement");
        }
    }
}
