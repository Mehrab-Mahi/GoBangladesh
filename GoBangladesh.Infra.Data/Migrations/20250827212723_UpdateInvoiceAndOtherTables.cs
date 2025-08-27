using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoBangladesh.Infra.Data.Migrations
{
    public partial class UpdateInvoiceAndOtherTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ToOrganizationId",
                table: "Invoices",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromOrganizationId",
                table: "Invoices",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceFilePath",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

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
                name: "IX_Invoices_FromOrganizationId",
                table: "Invoices",
                column: "FromOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ReceiverAccountId",
                table: "Invoices",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SenderAccountId",
                table: "Invoices",
                column: "SenderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ToOrganizationId",
                table: "Invoices",
                column: "ToOrganizationId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Organizations_FromOrganizationId",
                table: "Invoices",
                column: "FromOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Organizations_ToOrganizationId",
                table: "Invoices",
                column: "ToOrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Accounts_ReceiverAccountId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Accounts_SenderAccountId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Organizations_FromOrganizationId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Organizations_ToOrganizationId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_FromOrganizationId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ReceiverAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SenderAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ToOrganizationId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceFilePath",
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

            migrationBuilder.AlterColumn<string>(
                name: "ToOrganizationId",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromOrganizationId",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
