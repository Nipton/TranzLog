using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class Cargo_Change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Capacity",
                table: "Vehicles",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Vehicles",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
