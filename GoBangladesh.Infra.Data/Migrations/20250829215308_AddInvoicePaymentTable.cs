using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class AddInvoicePaymentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Accounts_ReceiverAccountId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Accounts_SenderAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ReceiverAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SenderAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentBy",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentProof",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReceivedBy",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReceivedTime",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentTime",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReceiverAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SenderAccountId",
                table: "Invoices");

            migrationBuilder.CreateTable(
                name: "InvoicePayment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentProof = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReceivedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentReceivedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SenderAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReceiverAccountId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicePayment_Accounts_ReceiverAccountId",
                        column: x => x.ReceiverAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicePayment_Accounts_SenderAccountId",
                        column: x => x.SenderAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicePayment_Users_PaymentBy",
                        column: x => x.PaymentBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoicePayment_Users_PaymentReceivedBy",
                        column: x => x.PaymentReceivedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayment_PaymentBy",
                table: "InvoicePayment",
                column: "PaymentBy");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayment_PaymentReceivedBy",
                table: "InvoicePayment",
                column: "PaymentReceivedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayment_ReceiverAccountId",
                table: "InvoicePayment",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayment_SenderAccountId",
                table: "InvoicePayment",
                column: "SenderAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoicePayment");

            migrationBuilder.AddColumn<string>(
                name: "PaymentBy",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProof",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReceivedBy",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentReceivedTime",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentTime",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverAccountId",
                table: "Invoices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderAccountId",
                table: "Invoices",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReceiverAccountId",
                table: "Invoices",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SenderAccountId",
                table: "Invoices",
                column: "SenderAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Accounts_ReceiverAccountId",
                table: "Invoices",
                column: "ReceiverAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Accounts_SenderAccountId",
                table: "Invoices",
                column: "SenderAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
