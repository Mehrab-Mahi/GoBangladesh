using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class UpdateAllTablesWithCardId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_PassengerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_PassengerId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PassengerId",
                table: "Trips",
                newName: "CardId");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_PassengerId",
                table: "Trips",
                newName: "IX_Trips_CardId");

            migrationBuilder.RenameColumn(
                name: "PassengerId",
                table: "Transactions",
                newName: "CardId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_PassengerId",
                table: "Transactions",
                newName: "IX_Transactions_CardId");

            migrationBuilder.CreateTable(
                name: "PassengerCardHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassengerCardHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassengerCardHistory_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PassengerCardHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PassengerCardHistory_CardId",
                table: "PassengerCardHistory",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PassengerCardHistory_UserId",
                table: "PassengerCardHistory",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Cards_CardId",
                table: "Transactions",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Cards_CardId",
                table: "Trips",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Cards_CardId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Cards_CardId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "PassengerCardHistory");

            migrationBuilder.RenameColumn(
                name: "CardId",
                table: "Trips",
                newName: "PassengerId");

            migrationBuilder.RenameIndex(
                name: "IX_Trips_CardId",
                table: "Trips",
                newName: "IX_Trips_PassengerId");

            migrationBuilder.RenameColumn(
                name: "CardId",
                table: "Transactions",
                newName: "PassengerId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CardId",
                table: "Transactions",
                newName: "IX_Transactions_PassengerId");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_PassengerId",
                table: "Transactions",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_PassengerId",
                table: "Trips",
                column: "PassengerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
