using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class TripModified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Sessions_SessionId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_SessionId",
                table: "Trips");

            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SessionId",
                table: "Trips",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_SessionId",
                table: "Trips",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Sessions_SessionId",
                table: "Trips",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
