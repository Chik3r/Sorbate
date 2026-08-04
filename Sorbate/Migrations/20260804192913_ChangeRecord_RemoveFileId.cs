using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sorbate.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRecord_RemoveFileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "ModRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileId",
                table: "ModRecords",
                type: "text",
                nullable: true);
        }
    }
}
