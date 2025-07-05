using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class UpdateTripTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TripStartPoint",
                table: "Trips",
                newName: "StartingLongitude");

            migrationBuilder.RenameColumn(
                name: "TripEndPoint",
                table: "Trips",
                newName: "StartingLatitude");

            migrationBuilder.AddColumn<string>(
                name: "EndingLatitude",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndingLongitude",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndingLatitude",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "EndingLongitude",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "StartingLongitude",
                table: "Trips",
                newName: "TripStartPoint");

            migrationBuilder.RenameColumn(
                name: "StartingLatitude",
                table: "Trips",
                newName: "TripEndPoint");
        }
    }
}
