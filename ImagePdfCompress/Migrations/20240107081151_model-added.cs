using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImagePdfCompress.Migrations
{
    /// <inheritdoc />
    public partial class modeladded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalSize = table.Column<long>(type: "bigint", nullable: false),
                    CompressedSize = table.Column<long>(type: "bigint", nullable: false),
                    CompressedPercentage = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Quality = table.Column<int>(type: "int", nullable: true),
                    OriginalFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompressedFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompressedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedFiles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedFiles");
        }
    }
}
