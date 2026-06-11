using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvenScan.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityLogAndAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_ActivityLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_AppSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_AppSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_ActivityLog_CreatedAt",
                table: "tb_ActivityLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tb_ActivityLog_UserId",
                table: "tb_ActivityLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_AppSetting_Key",
                table: "tb_AppSetting",
                column: "Key",
                unique: true);

            migrationBuilder.InsertData(
                table: "tb_AppSetting",
                columns: new[] { "Key", "Value", "UpdatedAt" },
                values: new object[] { "ActivityLog.AutoDeleteDays", "90", DateTime.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tb_ActivityLog");
            migrationBuilder.DropTable(name: "tb_AppSetting");
        }
    }
}
