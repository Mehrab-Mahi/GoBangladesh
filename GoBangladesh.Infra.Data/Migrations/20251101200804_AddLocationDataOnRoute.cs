using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddLocationDataOnRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TripEndLatitude",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TripEndLongitude",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TripStartLatitude",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TripStartLongitude",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TripEndLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TripEndLongitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TripStartLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "TripStartLongitude",
                table: "Routes");
        }
    }
}
