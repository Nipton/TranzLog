using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranzLog.Migrations
{
    /// <inheritdoc />
    public partial class ChangesFieldOrder2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Consignees_ConsigneeId",
                table: "TransportOrders");

            migrationBuilder.AlterColumn<int>(
                name: "ConsigneeId",
                table: "TransportOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Consignees_ConsigneeId",
                table: "TransportOrders",
                column: "ConsigneeId",
                principalTable: "Consignees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportOrders_Consignees_ConsigneeId",
                table: "TransportOrders");

            migrationBuilder.AlterColumn<int>(
                name: "ConsigneeId",
                table: "TransportOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TransportOrders_Consignees_ConsigneeId",
                table: "TransportOrders",
                column: "ConsigneeId",
                principalTable: "Consignees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
