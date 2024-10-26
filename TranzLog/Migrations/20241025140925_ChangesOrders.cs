using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class ChangesOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedDeliveryTime",
                table: "TransportOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTransportTime",
                table: "TransportOrders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDeliveryTime",
                table: "TransportOrders");

            migrationBuilder.DropColumn(
                name: "StartTransportTime",
                table: "TransportOrders");
        }
    }
}
