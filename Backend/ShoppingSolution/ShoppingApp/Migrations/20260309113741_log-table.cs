using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingApp.Migrations
{
    /// <inheritdoc />
    public partial class logtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErrorNumber",
                table: "Logs",
                newName: "RequestPath");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HttpMethod",
                table: "Logs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "HttpMethod",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "RequestPath",
                table: "Logs",
                newName: "ErrorNumber");
        }
    }
}
