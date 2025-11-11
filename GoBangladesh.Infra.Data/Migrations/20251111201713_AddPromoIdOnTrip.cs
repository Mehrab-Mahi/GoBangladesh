using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddPromoIdOnTrip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PromoId",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PromoId",
                table: "Trips");
        }
    }
}
