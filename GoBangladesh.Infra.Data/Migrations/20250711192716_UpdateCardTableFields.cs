using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class UpdateCardTableFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Cards");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Cards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "Cards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_OrganizationId",
                table: "Cards",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Organizations_OrganizationId",
                table: "Cards",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Organizations_OrganizationId",
                table: "Cards");

            migrationBuilder.DropIndex(
                name: "IX_Cards_OrganizationId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Cards");

            migrationBuilder.AddColumn<string>(
                name: "CardId",
                table: "Cards",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
