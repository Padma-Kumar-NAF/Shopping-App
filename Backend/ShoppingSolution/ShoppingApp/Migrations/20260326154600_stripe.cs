using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingApp.Migrations
{
    /// <inheritdoc />
    public partial class stripe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RazorpayPaymentId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "RazorpaySignature",
                table: "Payments",
                newName: "StripePaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripePaymentId",
                table: "Payments",
                newName: "RazorpaySignature");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
