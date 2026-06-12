using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvenScan.Migrations
{
    /// <inheritdoc />
    public partial class AddStockOutAndGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_StockOut",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockOut", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockOut_tb_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "tb_Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockOutDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockOutId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ScannedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockOutDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockOutDetail_tb_StockOut_StockOutId",
                        column: x => x.StockOutId,
                        principalTable: "tb_StockOut",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockOutDetail_tb_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "tb_Tag",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tb_StockOutDetail_tb_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "tb_Item",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_GateConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldMapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_GateConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_GateConfig_tb_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "tb_Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_GateLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateConfigId = table.Column<int>(type: "int", nullable: false),
                    EpcTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_GateLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_GateLog_tb_GateConfig_GateConfigId",
                        column: x => x.GateConfigId,
                        principalTable: "tb_GateConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockOut_DocNumber",
                table: "tb_StockOut",
                column: "DocNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockOut_LocationId",
                table: "tb_StockOut",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockOutDetail_StockOutId",
                table: "tb_StockOutDetail",
                column: "StockOutId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_GateConfig_ApiKey",
                table: "tb_GateConfig",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_GateConfig_GateCode",
                table: "tb_GateConfig",
                column: "GateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_GateLog_GateConfigId",
                table: "tb_GateLog",
                column: "GateConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_GateLog_ScannedAt",
                table: "tb_GateLog",
                column: "ScannedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tb_GateLog");
            migrationBuilder.DropTable(name: "tb_GateConfig");
            migrationBuilder.DropTable(name: "tb_StockOutDetail");
            migrationBuilder.DropTable(name: "tb_StockOut");
        }
    }
}
