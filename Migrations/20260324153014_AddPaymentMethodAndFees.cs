using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Namaya.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodAndFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseType",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FeeStatus",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSilambam",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsYoga",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FeeStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsSilambam",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsYoga",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");
        }
    }
}
