using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvenScan.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToStockTaking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "tb_StockTaking",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockTaking_LocationId",
                table: "tb_StockTaking",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_StockTaking_tb_Location_LocationId",
                table: "tb_StockTaking",
                column: "LocationId",
                principalTable: "tb_Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_StockTaking_tb_Location_LocationId",
                table: "tb_StockTaking");

            migrationBuilder.DropIndex(
                name: "IX_tb_StockTaking_LocationId",
                table: "tb_StockTaking");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "tb_StockTaking");
        }
    }
}
