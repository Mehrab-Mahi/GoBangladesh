using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddIndexingOnTripTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PassengerId",
                table: "Trips",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_PassengerId",
                table: "Trips",
                column: "PassengerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_PassengerId",
                table: "Trips",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_PassengerId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_PassengerId",
                table: "Trips");

            migrationBuilder.AlterColumn<string>(
                name: "PassengerId",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
