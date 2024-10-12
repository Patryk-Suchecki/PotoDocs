using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotoDocs.API.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                LastMyProperty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Role = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CompanyNIP = table.Column<int>(type: "int", nullable: false),
                CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Price = table.Column<float>(type: "real", nullable: false),
                DaysToPayment = table.Column<int>(type: "int", nullable: false),
                LoadingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                LoadingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UnloadingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UnloadingAddress = table.Column<DateTime>(type: "datetime2", nullable: false),
                CompanyOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                InvoiceIssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                PaymentMade = table.Column<bool>(type: "bit", nullable: false),
                DriverId = table.Column<int>(type: "int", nullable: false),
                PDFUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_Users_DriverId",
                    column: x => x.DriverId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CMRFiles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                OrderId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CMRFiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_CMRFiles_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CMRFiles_OrderId",
            table: "CMRFiles",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_DriverId",
            table: "Orders",
            column: "DriverId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CMRFiles");

        migrationBuilder.DropTable(
            name: "Orders");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
