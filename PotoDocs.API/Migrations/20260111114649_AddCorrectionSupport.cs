using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotoDocs.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalInvoiceId",
                table: "Invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OriginalInvoiceId",
                table: "Invoices",
                column: "OriginalInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Invoices_OriginalInvoiceId",
                table: "Invoices",
                column: "OriginalInvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Invoices_OriginalInvoiceId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_OriginalInvoiceId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OriginalInvoiceId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Invoices");
        }
    }
}
