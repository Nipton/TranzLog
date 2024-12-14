using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class RouteChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeliveryCost",
                table: "TransportOrders",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "Routes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "Routes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Routes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OriginLatitude",
                table: "Routes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OriginLongitude",
                table: "Routes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCost",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "OriginLatitude",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "OriginLongitude",
                table: "Routes");
        }
    }
}
