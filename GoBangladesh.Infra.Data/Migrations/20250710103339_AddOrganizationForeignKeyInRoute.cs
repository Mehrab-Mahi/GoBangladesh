using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddOrganizationForeignKeyInRoute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "Routes",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_OrganizationId",
                table: "Routes",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Organizations_OrganizationId",
                table: "Routes",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Organizations_OrganizationId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_OrganizationId",
                table: "Routes");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
