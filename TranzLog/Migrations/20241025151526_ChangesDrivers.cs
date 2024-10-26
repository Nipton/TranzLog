using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class ChangesDrivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Drivers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Drivers");
        }
    }
}
