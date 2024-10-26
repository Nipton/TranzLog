using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class CargoChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrackNumber",
                table: "TransportOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "Cargo",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "Cargo",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "Cargo",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_TrackNumber",
                table: "TransportOrders",
                column: "TrackNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransportOrders_TrackNumber",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Cargo");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "Cargo");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Cargo");

            migrationBuilder.AlterColumn<string>(
                name: "TrackNumber",
                table: "TransportOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
