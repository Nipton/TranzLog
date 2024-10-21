using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class OrderChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Cargo_CargoId",
                table: "TransportOrders");

            migrationBuilder.DropIndex(
                name: "IX_TransportOrders_CargoId",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "CargoId",
                table: "TransportOrders");

            migrationBuilder.AddColumn<string>(
                name: "TrackNumber",
                table: "TransportOrders",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TransportOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransportOrderId",
                table: "Cargo",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_TransportOrderId",
                table: "Cargo",
                column: "TransportOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cargo_TransportOrders_TransportOrderId",
                table: "Cargo",
                column: "TransportOrderId",
                principalTable: "TransportOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cargo_TransportOrders_TransportOrderId",
                table: "Cargo");

            migrationBuilder.DropIndex(
                name: "IX_Cargo_TransportOrderId",
                table: "Cargo");

            migrationBuilder.DropColumn(
                name: "TrackNumber",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "TransportOrderId",
                table: "Cargo");

            migrationBuilder.AddColumn<int>(
                name: "CargoId",
                table: "TransportOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportOrders_CargoId",
                table: "TransportOrders",
                column: "CargoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Cargo_CargoId",
                table: "TransportOrders",
                column: "CargoId",
                principalTable: "Cargo",
                principalColumn: "Id");
        }
    }
}
