using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCSortingCZP.Migrations
{
    /// <inheritdoc />
    public partial class NGBelt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NgBeltCellLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecordTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    FromInsideNum = table.Column<int>(type: "int", nullable: false),
                    FromStationName = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    ToInsideNum = table.Column<int>(type: "int", nullable: false),
                    ToStationName = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    CellCode = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    FromChannel = table.Column<int>(type: "int", nullable: false),
                    ToChannel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NgBeltCellLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NgBeltCellLogs");
        }
    }
}
