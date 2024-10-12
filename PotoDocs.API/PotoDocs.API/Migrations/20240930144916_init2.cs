using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotoDocs.API.Migrations;

/// <inheritdoc />
public partial class init2 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Password",
            table: "Users",
            newName: "PasswordHash");

        migrationBuilder.RenameColumn(
            name: "LastMyProperty",
            table: "Users",
            newName: "LastName");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "PasswordHash",
            table: "Users",
            newName: "Password");

        migrationBuilder.RenameColumn(
            name: "LastName",
            table: "Users",
            newName: "LastMyProperty");
    }
}
