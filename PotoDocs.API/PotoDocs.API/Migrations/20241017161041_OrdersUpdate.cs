using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotoDocs.API.Migrations
{
    /// <inheritdoc />
    public partial class OrdersUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Orders",
                newName: "PriceAmount");

            migrationBuilder.RenameColumn(
                name: "CompanyOrderNumber",
                table: "Orders",
                newName: "PriceCurrency");

            migrationBuilder.AlterColumn<string>(
                name: "UnloadingAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCountry",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompanyCountry",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "PriceCurrency",
                table: "Orders",
                newName: "CompanyOrderNumber");

            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "Orders",
                newName: "Price");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UnloadingAddress",
                table: "Orders",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
