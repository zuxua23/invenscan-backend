using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvenScan.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MinStock = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockPrep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockPrep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockTaking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockTaking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockIn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockIn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockIn_tb_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "tb_Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EpcTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_Tag_tb_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "tb_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_Tag_tb_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "tb_Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockPrepDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockPrepId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    RequestedQty = table.Column<int>(type: "int", nullable: false),
                    PickedQty = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScannedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockPrepDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockPrepDetail_tb_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "tb_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockPrepDetail_tb_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "tb_Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockPrepDetail_tb_StockPrep_StockPrepId",
                        column: x => x.StockPrepId,
                        principalTable: "tb_StockPrep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_StockInDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockInId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ScannedCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScanType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockInDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockInDetail_tb_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "tb_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockInDetail_tb_StockIn_StockInId",
                        column: x => x.StockInId,
                        principalTable: "tb_StockIn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockInDetail_tb_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "tb_Tag",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tb_StockTakingDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SttId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_StockTakingDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_StockTakingDetail_tb_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "tb_Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_tb_StockTakingDetail_tb_StockTaking_SttId",
                        column: x => x.SttId,
                        principalTable: "tb_StockTaking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_StockTakingDetail_tb_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "tb_Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_Item_ItemCode",
                table: "tb_Item",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Location_LocationCode",
                table: "tb_Location",
                column: "LocationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockIn_DocNumber",
                table: "tb_StockIn",
                column: "DocNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockIn_LocationId",
                table: "tb_StockIn",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockInDetail_ItemId",
                table: "tb_StockInDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockInDetail_StockInId",
                table: "tb_StockInDetail",
                column: "StockInId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockInDetail_TagId",
                table: "tb_StockInDetail",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockPrep_DocNumber",
                table: "tb_StockPrep",
                column: "DocNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockPrepDetail_ItemId",
                table: "tb_StockPrepDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockPrepDetail_LocationId",
                table: "tb_StockPrepDetail",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockPrepDetail_StockPrepId",
                table: "tb_StockPrepDetail",
                column: "StockPrepId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockTaking_SessionCode",
                table: "tb_StockTaking",
                column: "SessionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockTakingDetail_ItemId",
                table: "tb_StockTakingDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockTakingDetail_SttId",
                table: "tb_StockTakingDetail",
                column: "SttId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_StockTakingDetail_TagId",
                table: "tb_StockTakingDetail",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tag_EpcTag",
                table: "tb_Tag",
                column: "EpcTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tag_ItemId",
                table: "tb_Tag",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tag_LocationId",
                table: "tb_Tag",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_Tag_TagId",
                table: "tb_Tag",
                column: "TagId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_User_UserId",
                table: "tb_User",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_StockInDetail");

            migrationBuilder.DropTable(
                name: "tb_StockPrepDetail");

            migrationBuilder.DropTable(
                name: "tb_StockTakingDetail");

            migrationBuilder.DropTable(
                name: "tb_User");

            migrationBuilder.DropTable(
                name: "tb_StockIn");

            migrationBuilder.DropTable(
                name: "tb_StockPrep");

            migrationBuilder.DropTable(
                name: "tb_StockTaking");

            migrationBuilder.DropTable(
                name: "tb_Tag");

            migrationBuilder.DropTable(
                name: "tb_Item");

            migrationBuilder.DropTable(
                name: "tb_Location");
        }
    }
}
