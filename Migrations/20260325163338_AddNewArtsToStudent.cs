using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Namaya.Migrations
{
    /// <inheritdoc />
    public partial class AddNewArtsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBoxing",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGymnastics",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsKarate",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBoxing",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsGymnastics",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsKarate",
                table: "Students");
        }
    }
}
