using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddLineStringOnRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinimumDistanceInMeterFromRoute",
                table: "SystemSettings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<LineString>(
                name: "RoutePath",
                table: "Routes",
                type: "geography",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumDistanceInMeterFromRoute",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RoutePath",
                table: "Routes");
        }
    }
}
