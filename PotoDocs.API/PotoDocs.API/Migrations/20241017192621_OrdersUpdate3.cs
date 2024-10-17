using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotoDocs.API.Migrations
{
    /// <inheritdoc />
    public partial class OrdersUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceCurrency",
                table: "Orders",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "Orders",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "PaymentMade",
                table: "Orders",
                newName: "HasPaid");

            migrationBuilder.RenameColumn(
                name: "DaysToPayment",
                table: "Orders",
                newName: "PaymentDeadline");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "Orders",
                newName: "CompanyOrderNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Orders",
                newName: "PriceAmount");

            migrationBuilder.RenameColumn(
                name: "PaymentDeadline",
                table: "Orders",
                newName: "DaysToPayment");

            migrationBuilder.RenameColumn(
                name: "HasPaid",
                table: "Orders",
                newName: "PaymentMade");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "Orders",
                newName: "PriceCurrency");

            migrationBuilder.RenameColumn(
                name: "CompanyOrderNumber",
                table: "Orders",
                newName: "Comments");
        }
    }
}
