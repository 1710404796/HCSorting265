using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCSortingCZP.Migrations
{
    /// <inheritdoc />
    public partial class addpalllog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OKNGCnts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OKCnt = table.Column<int>(type: "int", nullable: false),
                    NG1Cnt = table.Column<int>(type: "int", nullable: false),
                    NG2Cnt = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OKNGCnts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PalletLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PalletCode = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ZuPanTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Column = table.Column<int>(type: "int", nullable: false),
                    PalletType = table.Column<int>(type: "int", nullable: false),
                    BatteryInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OKNGCnts");

            migrationBuilder.DropTable(
                name: "PalletLogs");
        }
    }
}
